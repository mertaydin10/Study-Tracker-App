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
    $("subjects").innerHTML = subjects
      .map(
        (s) =>
          `<li>${escapeHtml(s.name)} (#${s.id}) <button type="button" class="small" data-delete-subject="${s.id}">Sil</button></li>`
      )
      .join("");
    $("subjectId").innerHTML = subjects
      .map((s) => `<option value="${s.id}">${escapeHtml(s.name)}</option>`)
      .join("");

    const tags = await api("/api/tags");
    $("tags").innerHTML = tags
      .map(
        (t) =>
          `<li>${escapeHtml(t.name)} <button type="button" class="small" data-delete-tag="${t.id}">Sil</button></li>`
      )
      .join("");
    $("tagOptions").innerHTML = tags
      .map(
        (t) =>
          `<label><input type="checkbox" name="tag" value="${t.id}" /> ${escapeHtml(t.name)}</label>`
      )
      .join("");

    const sessions = await api("/api/sessions?page=1&pageSize=20");
    $("sessions").innerHTML = (sessions.items || [])
      .map((s) => {
        const tagText = (s.tags || []).map((t) => t.name).join(", ");
        const extra = tagText ? ` [${escapeHtml(tagText)}]` : "";
        return `<li>${escapeHtml(s.subjectName)} — ${s.durationMinutes} dk${extra}
          <button type="button" class="small" data-delete-session="${s.id}">Sil</button></li>`;
      })
      .join("");

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
    await api("/api/sessions", {
      method: "POST",
      body: JSON.stringify({
        subjectId: Number($("subjectId").value),
        startedAt: new Date().toISOString(),
        durationMinutes: Number($("duration").value),
        notes: $("notes").value || null,
        tagIds: [...document.querySelectorAll("#tagOptions input:checked")].map(
          (el) => Number(el.value)
        )
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
$("tagForm").addEventListener("submit", addTag);
$("sessionForm").addEventListener("submit", addSession);

$("app").addEventListener("click", async (event) => {
  const target = event.target;
  if (!(target instanceof HTMLElement)) return;
  try {
    if (target.dataset.deleteSubject) {
      await api(`/api/subjects/${target.dataset.deleteSubject}`, { method: "DELETE" });
      await refresh();
    } else if (target.dataset.deleteTag) {
      await api(`/api/tags/${target.dataset.deleteTag}`, { method: "DELETE" });
      await refresh();
    } else if (target.dataset.deleteSession) {
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
