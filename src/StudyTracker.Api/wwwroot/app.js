const tokenKey = "studyTrackerToken";

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
  const data = text ? JSON.parse(text) : null;
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

function logout() {
  localStorage.removeItem(tokenKey);
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

async function refresh() {
  $("error").textContent = "";
  try {
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

    const filter = $("sessionFilter").value;
    const qs = filter
      ? `/api/sessions?page=1&pageSize=20&subjectId=${encodeURIComponent(filter)}`
      : "/api/sessions?page=1&pageSize=20";
    const sessions = await api(qs);
    const items = sessions.items || [];
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

    const stats = await api("/api/stats/summary");
    $("stats").textContent =
      `${stats.sessionCount} oturum, ${stats.totalMinutes} dakika`;
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
        startedAt: new Date().toISOString(),
        durationMinutes: Number($("duration").value),
        notes: $("notes").value || null,
        tagIds: []
      })
    });
    $("notes").value = "";
    await refresh();
  } catch (e) {
    $("error").textContent = e.message;
  }
}

$("loginForm").addEventListener("submit", login);
$("logout").addEventListener("click", logout);
$("subjectForm").addEventListener("submit", addSubject);
$("sessionForm").addEventListener("submit", addSession);
$("sessionFilter").addEventListener("change", () => refresh());

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
      if (!Number.isInteger(durationMinutes) || durationMinutes < 1) {
        $("error").textContent = "Dakika 1 veya daha büyük olmalı.";
        return;
      }
      const notes = window.prompt("Not", target.dataset.notes ?? "");
      if (notes === null) return;
      await api(`/api/sessions/${target.dataset.editSession}`, {
        method: "PUT",
        body: JSON.stringify({
          subjectId: Number(target.dataset.subjectId),
          startedAt: target.dataset.started,
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
  refresh();
} else {
  showApp(false);
}
