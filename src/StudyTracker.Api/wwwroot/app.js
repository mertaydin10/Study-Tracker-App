let sessionPage = 1;
const sessionPageSize = 10;
const tokenKey = "studyTrackerToken";

function toLocalInputValue(date = new Date()) {
  const pad = (n) => String(n).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

const $ = (id) => document.getElementById(id);

function token() {
  return localStorage.getItem(tokenKey);
}

async function api(path, options = {}) {
  const headers = { ...(options.headers || {}) };
  if (options.body && !headers["Content-Type"]) {
    headers["Content-Type"] = "application/json";
  }
  const t = token();
  if (t) headers.Authorization = `Bearer ${t}`;

  const res = await fetch(path, { ...options, headers });
  if (res.status === 204) return null;
  const text = await res.text();
  let data = null;
  if (text) {
    try {
      data = JSON.parse(text);
    } catch {
      throw new Error(res.statusText || "İstek başarısız.");
    }
  }
  if (!res.ok) {
    const msg = data?.error || data?.title || res.statusText;
    throw new Error(msg);
  }
  return data;
}

function showApp(loggedIn) {
  $("login").classList.toggle("hidden", loggedIn);
  $("app").classList.toggle("hidden", !loggedIn);
}

async function login(event) {
  event.preventDefault();
  $("error").textContent = "";
  try {
    const data = await api("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({
        email: $("email").value,
        password: $("password").value
      })
    });
    localStorage.setItem(tokenKey, data.token);
    showApp(true);
    await refresh();
  } catch (e) {
    $("error").textContent = e.message;
  }
}

async function register(event) {
  event.preventDefault();
  $("error").textContent = "";
  try {
    const data = await api("/api/auth/register", {
      method: "POST",
      body: JSON.stringify({
        email: $("regEmail").value,
        password: $("regPassword").value,
        displayName: $("regName").value || null
      })
    });
    localStorage.setItem(tokenKey, data.token);
    showApp(true);
    await refresh();
  } catch (e) {
    $("error").textContent = e.message;
  }
}

function logout() {
  localStorage.removeItem(tokenKey);
  $("notice").textContent = "";
  $("error").textContent = "";
  showApp(false);
}

function formatWhen(iso) {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return "";
  return d.toLocaleString("tr-TR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit"
  });
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;");
}

function toDateInput(date) {
  const pad = (n) => String(n).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

function applyDateRange(from, to) {
  $("fromDate").value = from;
  $("toDate").value = to;
  sessionPage = 1;
  refresh();
}

function applyFilters(qs) {
  const filter = $("sessionFilter").value;
  if (filter) qs.set("subjectId", filter);
  const from = $("fromDate").value;
  const to = $("toDate").value;
  if (from && to && from > to) return false;
  if (from) qs.set("from", new Date(`${from}T00:00:00`).toISOString());
  if (to) qs.set("to", new Date(`${to}T23:59:59`).toISOString());
  return true;
}

async function refresh() {
  $("error").textContent = "";
  try {
    const me = await api("/api/auth/me");
    $("who").textContent = me.displayName || me.email;
    const subjects = await api("/api/subjects");
    $("subjects").innerHTML = subjects.length
      ? subjects
          .map(
            (s, i) =>
              `<li>${i + 1}. ${escapeHtml(s.name)}
              <button type="button" class="small" data-rename-subject="${s.id}" data-name="${escapeHtml(s.name)}">Adı değiştir</button>
              <button type="button" class="small" data-delete-subject="${s.id}">Sil</button></li>`
          )
          .join("")
      : `<li class="empty">Henüz konu yok.</li>`;

    $("sessionHint").classList.toggle("hidden", subjects.length > 0);
    for (const el of $("sessionForm").querySelectorAll("input, select, button")) {
      el.disabled = subjects.length === 0;
    }
    const selectedAdd = $("subjectId").value;
    const selectedFilter = $("sessionFilter").value;
    $("subjectId").innerHTML = subjects
      .map((s) => `<option value="${s.id}">${escapeHtml(s.name)}</option>`)
      .join("");
    if (selectedAdd) $("subjectId").value = selectedAdd;

    $("sessionFilter").innerHTML =
      `<option value="">Tümü</option>` +
      subjects
        .map((s) => `<option value="${s.id}">${escapeHtml(s.name)}</option>`)
        .join("");
    if (selectedFilter) $("sessionFilter").value = selectedFilter;

    const qs = new URLSearchParams({
      page: String(sessionPage),
      pageSize: String(sessionPageSize)
    });
    if (!applyFilters(qs)) {
      $("error").textContent = "Başlangıç bitişten sonra olamaz.";
      return;
    }
    const sessions = await api(`/api/sessions?${qs}`);
    const items = sessions.items || [];
    const total = sessions.totalCount ?? 0;
    const lastPage = Math.max(1, Math.ceil(total / sessionPageSize));
    if (sessionPage > lastPage) {
      sessionPage = lastPage;
      await refresh();
      return;
    }
    $("pageLabel").textContent = `${sessionPage} / ${lastPage} (${total})`;
    $("prevPage").disabled = sessionPage <= 1;
    $("nextPage").disabled = sessionPage >= lastPage;
    $("sessions").innerHTML = items.length
      ? items
          .map((s) => {
            const note = s.notes ? ` — ${escapeHtml(s.notes)}` : "";
            return `<li>${escapeHtml(s.subjectName)} — ${s.durationMinutes} dk
              <span class="when">${escapeHtml(formatWhen(s.startedAt))}</span>${note}
              <button type="button" class="small" data-edit-session="${s.id}"
                data-subject-id="${s.subjectId}" data-started="${escapeHtml(s.startedAt)}"
                data-duration="${s.durationMinutes}" data-notes="${escapeHtml(s.notes ?? "")}">Düzenle</button>
              <button type="button" class="small" data-delete-session="${s.id}">Sil</button></li>`;
          })
          .join("")
      : `<li class="empty">Bu filtrede oturum yok.</li>`;

    const statsQs = new URLSearchParams();
    applyFilters(statsQs);
    const statsPath = statsQs.toString()
      ? `/api/stats/summary?${statsQs}`
      : "/api/stats/summary";
    const stats = await api(statsPath);
    $("stats").textContent =
      `${stats.sessionCount} oturum, ${stats.totalMinutes} dakika`;
    $("statsBySubject").innerHTML = (stats.bySubject || []).length
      ? stats.bySubject
          .map((row) => `<li>${escapeHtml(row.subjectName)}: ${row.sessionCount} oturum, ${row.totalMinutes} dk</li>`)
          .join("")
      : `<li class="empty">Bu aralıkta özet yok.</li>`;
  } catch (e) {
    $("error").textContent = e.message;
    if (String(e.message).includes("Unauthorized") || e.message === "Unauthorized") {
      logout();
    }
  }
}

async function addSubject(event) {
  event.preventDefault();
  try {
    await api("/api/subjects", {
      method: "POST",
      body: JSON.stringify({ name: $("subjectName").value })
    });
    $("subjectName").value = "";
    await refresh();
  } catch (e) {
    $("error").textContent = e.message;
  }
}

async function addSession(event) {
  event.preventDefault();
  try {
    await api("/api/sessions", {
      method: "POST",
      body: JSON.stringify({
        subjectId: Number($("subjectId").value),
        startedAt: new Date($("startedAt").value).toISOString(),
        durationMinutes: Number($("duration").value),
        notes: $("notes").value || null,
        tagIds: []
      })
    });
    $("notes").value = "";
    $("startedAt").value = toLocalInputValue();
    await refresh();
  } catch (e) {
    $("error").textContent = e.message;
  }
}

$("loginForm").addEventListener("submit", login);
$("registerForm").addEventListener("submit", register);
$("logout").addEventListener("click", logout);
$("subjectForm").addEventListener("submit", addSubject);
$("sessionForm").addEventListener("submit", addSession);
$("sessionFilter").addEventListener("change", () => {
  sessionPage = 1;
  refresh();
});
$("fromDate").addEventListener("change", () => {
  sessionPage = 1;
  refresh();
});
$("toDate").addEventListener("change", () => {
  sessionPage = 1;
  refresh();
});
$("clearDates").addEventListener("click", () => {
  $("fromDate").value = "";
  $("toDate").value = "";
  sessionPage = 1;
  refresh();
});
$("filterToday").addEventListener("click", () => {
  const today = toDateInput(new Date());
  applyDateRange(today, today);
});
$("filterWeek").addEventListener("click", () => {
  const now = new Date();
  const mondayOffset = (now.getDay() + 6) % 7;
  const monday = new Date(now);
  monday.setDate(now.getDate() - mondayOffset);
  applyDateRange(toDateInput(monday), toDateInput(now));
});
$("changePassword").addEventListener("click", async () => {
  $("error").textContent = "";
  const currentPassword = window.prompt("Mevcut şifre");
  if (currentPassword === null || currentPassword === "") return;
  const newPassword = window.prompt("Yeni şifre (en az 4 karakter)");
  if (newPassword === null) return;
  if (newPassword.length < 4) {
    $("error").textContent = "Yeni şifre en az 4 karakter olmalı.";
    return;
  }
  try {
    await api("/api/auth/change-password", {
      method: "POST",
      body: JSON.stringify({ currentPassword, newPassword })
    });
    $("notice").textContent = "Şifre güncellendi.";
  } catch (e) {
    $("error").textContent = e.message;
  }
});
$("renameMe").addEventListener("click", async () => {
  $("error").textContent = "";
  const current = $("who").textContent;
  const name = window.prompt("Görünen ad", current);
  if (!name || !name.trim()) return;
  try {
    const me = await api("/api/auth/me", {
      method: "PUT",
      body: JSON.stringify({ displayName: name.trim() })
    });
    $("who").textContent = me.displayName || me.email;
  } catch (e) {
    $("error").textContent = e.message;
  }
});
$("prevPage").addEventListener("click", () => {
  if (sessionPage > 1) {
    sessionPage -= 1;
    refresh();
  }
});
$("nextPage").addEventListener("click", () => {
  sessionPage += 1;
  refresh();
});
$("exportCsv").addEventListener("click", async () => {
  $("error").textContent = "";
  try {
    const qs = new URLSearchParams();
    if (!applyFilters(qs)) {
      $("error").textContent = "Başlangıç bitişten sonra olamaz.";
      return;
    }
    const suffix = qs.toString() ? `?${qs}` : "";
    const res = await fetch(`/api/sessions/export${suffix}`, {
      headers: { Authorization: `Bearer ${token()}` }
    });
    if (!res.ok) throw new Error("CSV indirilemedi.");
    const blob = await res.blob();
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "sessions.csv";
    a.click();
    URL.revokeObjectURL(url);
  } catch (e) {
    $("error").textContent = e.message;
  }
});

$("app").addEventListener("click", async (event) => {
  const target = event.target;
  if (!(target instanceof HTMLElement)) return;
  try {
    if (target.dataset.renameSubject) {
      const current = target.dataset.name ?? "";
      const name = window.prompt("Yeni konu adı", current);
      if (!name || !name.trim()) return;
      await api(`/api/subjects/${target.dataset.renameSubject}`, {
        method: "PUT",
        body: JSON.stringify({ name: name.trim() })
      });
      await refresh();
    } else if (target.dataset.editSession) {
      const minutes = window.prompt("Dakika", target.dataset.duration ?? "25");
      if (minutes === null) return;
      const durationMinutes = Number(minutes);
      if (!Number.isInteger(durationMinutes) || durationMinutes < 1 || durationMinutes > 1440) {
        $("error").textContent = "Dakika 1 ile 1440 arasında olmalı.";
        return;
      }
      const started = window.prompt(
        "Başlangıç (YYYY-MM-DDTHH:MM)",
        toLocalInputValue(new Date(target.dataset.started))
      );
      if (started === null) return;
      const startedAt = new Date(started);
      if (Number.isNaN(startedAt.getTime())) {
        $("error").textContent = "Geçerli bir tarih-saat yaz.";
        return;
      }
      const notes = window.prompt("Not", target.dataset.notes ?? "");
      if (notes === null) return;
      await api(`/api/sessions/${target.dataset.editSession}`, {
        method: "PUT",
        body: JSON.stringify({
          subjectId: Number(target.dataset.subjectId),
          startedAt: startedAt.toISOString(),
          durationMinutes,
          notes: notes.trim() || null,
          tagIds: []
        })
      });
      await refresh();
    } else if (target.dataset.deleteSubject) {
      if (!window.confirm("Bu konuyu silmek istiyor musun?")) return;
      await api(`/api/subjects/${target.dataset.deleteSubject}`, { method: "DELETE" });
      await refresh();
    } else if (target.dataset.deleteSession) {
      if (!window.confirm("Bu oturumu silmek istiyor musun?")) return;
      await api(`/api/sessions/${target.dataset.deleteSession}`, { method: "DELETE" });
      await refresh();
    }
  } catch (e) {
    $("error").textContent = e.message;
  }
});

if (token()) {
  showApp(true);
  $("startedAt").value = toLocalInputValue();
  refresh();
} else {
  showApp(false);
  $("startedAt").value = toLocalInputValue();
}
