let sessionPage = 1;
let sessionPageSize = 10;
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
    const err = new Error(data?.error || data?.title || res.statusText);
    err.status = res.status;
    throw err;
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
  const tagId = $("tagFilter").value;
  if (tagId) qs.set("tagId", tagId);
  return true;
}

function selectedTagIds() {
  return [...$("sessionTags").selectedOptions].map((o) => Number(o.value));
}

function resetSessionForm() {
  $("editingSessionId").value = "";
  $("sessionSubmit").textContent = "Oturum ekle";
  $("cancelEdit").classList.add("hidden");
  $("notes").value = "";
  $("duration").value = "25";
  $("startedAt").value = toLocalInputValue();
  for (const option of $("sessionTags").options) option.selected = false;
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

    const tags = await api("/api/tags");
    $("tags").innerHTML = tags.length
      ? tags
          .map(
            (t) =>
              `<li>${escapeHtml(t.name)}
              <button type="button" class="small" data-delete-tag="${t.id}">Sil</button></li>`
          )
          .join("")
      : `<li class="empty">Henüz etiket yok.</li>`;

    $("sessionHint").classList.toggle("hidden", subjects.length > 0);
    for (const el of $("sessionForm").querySelectorAll("input, select, button")) {
      if (el.id === "cancelEdit") continue;
      el.disabled = subjects.length === 0;
    }
    const selectedAdd = $("subjectId").value;
    const selectedFilter = $("sessionFilter").value;
    const selectedTagFilter = $("tagFilter").value;
    const selectedSessionTags = selectedTagIds();
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

    $("tagFilter").innerHTML =
      `<option value="">Tümü</option>` +
      tags.map((t) => `<option value="${t.id}">${escapeHtml(t.name)}</option>`).join("");
    if (selectedTagFilter) $("tagFilter").value = selectedTagFilter;

    $("sessionTags").innerHTML = tags
      .map((t) => `<option value="${t.id}">${escapeHtml(t.name)}</option>`)
      .join("");
    for (const option of $("sessionTags").options) {
      option.selected = selectedSessionTags.includes(Number(option.value));
    }

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
            const tagNames = (s.tags || []).map((t) => t.name).join(", ");
            const tagPart = tagNames ? ` [${escapeHtml(tagNames)}]` : "";
            const tagIds = (s.tags || []).map((t) => t.id).join(",");
            return `<li>${escapeHtml(s.subjectName)} — ${s.durationMinutes} dk
              <span class="when">${escapeHtml(formatWhen(s.startedAt))}</span>${note}${tagPart}
              <button type="button" class="small" data-edit-session="${s.id}"
                data-subject-id="${s.subjectId}" data-started="${escapeHtml(s.startedAt)}"
                data-duration="${s.durationMinutes}" data-notes="${escapeHtml(s.notes ?? "")}"
                data-tag-ids="${escapeHtml(tagIds)}">Düzenle</button>
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
    if (e.status === 401) {
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

async function addTag(event) {
  event.preventDefault();
  try {
    await api("/api/tags", {
      method: "POST",
      body: JSON.stringify({ name: $("tagName").value })
    });
    $("tagName").value = "";
    await refresh();
  } catch (e) {
    $("error").textContent = e.message;
  }
}

async function addSession(event) {
  event.preventDefault();
  try {
    const id = $("editingSessionId").value;
    const body = {
      subjectId: Number($("subjectId").value),
      startedAt: new Date($("startedAt").value).toISOString(),
      durationMinutes: Number($("duration").value),
      notes: $("notes").value || null,
      tagIds: selectedTagIds()
    };
    if (id) {
      await api(`/api/sessions/${id}`, { method: "PUT", body: JSON.stringify(body) });
    } else {
      await api("/api/sessions", { method: "POST", body: JSON.stringify(body) });
    }
    resetSessionForm();
    await refresh();
  } catch (e) {
    $("error").textContent = e.message;
  }
}

$("loginForm").addEventListener("submit", login);
$("registerForm").addEventListener("submit", register);
$("logout").addEventListener("click", logout);
$("subjectForm").addEventListener("submit", addSubject);
$("tagForm").addEventListener("submit", addTag);
$("sessionForm").addEventListener("submit", addSession);
$("sessionFilter").addEventListener("change", () => {
  sessionPage = 1;
  refresh();
});
$("tagFilter").addEventListener("change", () => {
  sessionPage = 1;
  refresh();
});
$("pageSize").addEventListener("change", () => {
  sessionPageSize = Number($("pageSize").value) || 10;
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
$("cancelEdit").addEventListener("click", () => {
  resetSessionForm();
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
      $("editingSessionId").value = target.dataset.editSession;
      $("subjectId").value = target.dataset.subjectId ?? "";
      $("duration").value = target.dataset.duration ?? "25";
      $("notes").value = target.dataset.notes ?? "";
      $("startedAt").value = toLocalInputValue(new Date(target.dataset.started));
      const ids = (target.dataset.tagIds || "")
        .split(",")
        .map((x) => Number(x))
        .filter((n) => n);
      for (const option of $("sessionTags").options) {
        option.selected = ids.includes(Number(option.value));
      }
      $("sessionSubmit").textContent = "Kaydet";
      $("cancelEdit").classList.remove("hidden");
      $("sessionForm").scrollIntoView({ behavior: "smooth" });
    } else if (target.dataset.deleteTag) {
      if (!window.confirm("Bu etiketi silmek istiyor musun?")) return;
      await api(`/api/tags/${target.dataset.deleteTag}`, { method: "DELETE" });
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

(async function pingHealth() {
  try {
    const res = await fetch("/health");
    const data = await res.json();
    $("health").textContent = data.database === "up" ? "" : "Veritabanına bağlanılamıyor.";
  } catch {
    $("health").textContent = "API’ye ulaşılamıyor.";
  }
})();
