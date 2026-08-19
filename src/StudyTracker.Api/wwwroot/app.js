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

async function refresh() {
  $("error").textContent = "";
  try {
    const subjects = await api("/api/subjects");
    $("subjects").innerHTML = subjects
      .map((s) => `<li>${s.name} (#${s.id})</li>`)
      .join("");
    $("subjectId").innerHTML = subjects
      .map((s) => `<option value="${s.id}">${s.name}</option>`)
      .join("");

    const sessions = await api("/api/sessions?page=1&pageSize=20");
    $("sessions").innerHTML = (sessions.items || [])
      .map(
        (s) =>
          `<li>${s.subjectName} — ${s.durationMinutes} dk (${s.startedAt})</li>`
      )
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

if (token()) {
  showApp(true);
  refresh();
} else {
  showApp(false);
}
