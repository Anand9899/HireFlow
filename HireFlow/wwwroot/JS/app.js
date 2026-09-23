// =====================================================
// HireFlow - Ultra-Premium Next-Gen Frontend Application
// =====================================================
"use strict";

const API_BASE_URL = "/api";
const TOKEN_KEY = "hireflow_token";
const THEME_KEY = "hireflow_theme";
const SAVED_JOBS_KEY = "hireflow_saved_jobs";

// Global cache & state
let allJobsCache = [];
let activeJobTypeFilter = "all";
let showingSavedOnly = false;
let currentDecodedUser = null;
let searchDebounceTimer = null;

// =====================================================
// HELPER UTILITIES
// =====================================================
function $(id) {
    return document.getElementById(id);
}

function escapeHtml(value) {
    const div = document.createElement("div");
    div.textContent = value ?? "";
    return div.innerHTML;
}

// =====================================================
// THEME ENGINE (LIGHT / DARK)
// =====================================================
function initTheme() {
    const savedTheme = localStorage.getItem(THEME_KEY);
    const prefersDark = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;
    const theme = savedTheme || (prefersDark ? "dark" : "light");

    applyTheme(theme);

    const toggleBtn = $("themeToggleBtn");
    if (toggleBtn) {
        toggleBtn.addEventListener("click", toggleTheme);
    }
}

function applyTheme(theme) {
    document.documentElement.setAttribute("data-theme", theme);
    localStorage.setItem(THEME_KEY, theme);

    const toggleBtn = $("themeToggleBtn");
    if (toggleBtn) {
        if (theme === "dark") {
            toggleBtn.textContent = "☀️";
            toggleBtn.title = "Switch to Light Mode";
            toggleBtn.setAttribute("aria-label", "Switch to Light Mode");
        } else {
            toggleBtn.textContent = "🌙";
            toggleBtn.title = "Switch to Dark Mode";
            toggleBtn.setAttribute("aria-label", "Switch to Dark Mode");
        }
    }
}

function toggleTheme() {
    const current = document.documentElement.getAttribute("data-theme") || "light";
    const next = current === "dark" ? "light" : "dark";
    applyTheme(next);
    showToast(`Switched to ${next === "dark" ? "Dark 🌙" : "Light ☀️"} mode`, "info");
}

// =====================================================
// TOAST NOTIFICATIONS
// =====================================================
function showToast(message, type = "info") {
    const container = $("toastContainer");
    if (!container) return;

    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;
    const icon = type === "success" ? "✅" : (type === "error" ? "❌" : "ℹ️");
    toast.innerHTML = `<span>${icon}</span><span>${escapeHtml(message)}</span>`;
    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = "0";
        toast.style.transform = "translateX(50px)";
        setTimeout(() => toast.remove(), 350);
    }, 3800);
}

// =====================================================
// TOKEN & SESSION MANAGEMENT
// =====================================================
function getToken() {
    return sessionStorage.getItem(TOKEN_KEY);
}

function saveToken(token) {
    if (!token) return;
    sessionStorage.setItem(TOKEN_KEY, token);
    localStorage.removeItem(TOKEN_KEY);
}

function clearLoginSession() {
    sessionStorage.removeItem(TOKEN_KEY);
    currentDecodedUser = null;
}

function getAuthHeaders() {
    const token = getToken();
    return token ? { "Authorization": `Bearer ${token}` } : {};
}

function isTokenExpired(token) {
    if (!token) return true;
    try {
        const parts = token.split(".");
        if (parts.length !== 3) return true;

        let base64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
        while (base64.length % 4 !== 0) base64 += "=";

        const payload = JSON.parse(atob(base64));
        if (!payload.exp) return false;

        const now = Math.floor(Date.now() / 1000);
        return payload.exp <= now;
    } catch {
        return true;
    }
}

function isLoggedIn() {
    const token = getToken();
    if (!token) return false;
    if (isTokenExpired(token)) {
        clearLoginSession();
        return false;
    }
    return true;
}

function getUserFromToken() {
    const token = getToken();
    if (!token || isTokenExpired(token)) return null;

    try {
        const parts = token.split(".");
        let base64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
        while (base64.length % 4 !== 0) base64 += "=";
        const payload = JSON.parse(atob(base64));

        const role = payload.role
            || payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
            || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role"]
            || "Candidate";

        const name = payload.name
            || payload.unique_name
            || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]
            || "User";

        const email = payload.email
            || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"]
            || "";

        const id = payload.nameid
            || payload.sub
            || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]
            || "";

        return { id, name, email, role };
    } catch {
        return null;
    }
}

// =====================================================
// SAVED / BOOKMARKED JOBS ENGINE
// =====================================================
function getSavedJobIds() {
    try {
        const stored = localStorage.getItem(SAVED_JOBS_KEY);
        return stored ? JSON.parse(stored) : [];
    } catch {
        return [];
    }
}

function setSavedJobIds(ids) {
    try {
        localStorage.setItem(SAVED_JOBS_KEY, JSON.stringify(ids));
        updateSavedJobsCount();
    } catch (e) {
        console.error("Could not write saved jobs:", e);
    }
}

function isJobSaved(jobId) {
    const saved = getSavedJobIds();
    return saved.includes(Number(jobId));
}

function toggleSaveJob(event, jobId) {
    if (event) {
        event.stopPropagation();
    }
    const id = Number(jobId);
    let saved = getSavedJobIds();
    const index = saved.indexOf(id);

    if (index >= 0) {
        saved.splice(index, 1);
        setSavedJobIds(saved);
        showToast("Job removed from saved bookmarks", "info");
    } else {
        saved.push(id);
        setSavedJobIds(saved);
        showToast("Job saved to your bookmarks! ❤️", "success");
    }

    renderFilteredJobs();
}

function updateSavedJobsCount() {
    const countEl = $("savedJobsCount");
    if (countEl) {
        const count = getSavedJobIds().length;
        countEl.textContent = count;
    }
}

function toggleViewSavedJobs() {
    showingSavedOnly = !showingSavedOnly;
    const btn = $("viewSavedJobsBtn");

    if (btn) {
        if (showingSavedOnly) {
            btn.classList.add("btn-primary");
            btn.classList.remove("btn-outline");
            btn.innerHTML = `❤️ Showing Saved (<span id="savedJobsCount">${getSavedJobIds().length}</span>) ✕`;
        } else {
            btn.classList.remove("btn-primary");
            btn.classList.add("btn-outline");
            btn.innerHTML = `❤️ Saved Jobs (<span id="savedJobsCount">${getSavedJobIds().length}</span>)`;
        }
    }

    renderFilteredJobs();
}

// =====================================================
// MODAL CONTROLS
// =====================================================
function openModal(id) {
    const m = $(id);
    if (m) {
        m.classList.add("active");
        document.body.classList.add("modal-open");
    }
}

function closeModal(id) {
    const m = $(id);
    if (m) {
        m.classList.remove("active");
        document.body.classList.remove("modal-open");
    }
}

// =====================================================
// QUICK VIEW / JOB DETAILS MODAL
// =====================================================
window.openJobDetails = function (jobId) {
    const job = allJobsCache.find(j => j.id === Number(jobId));
    if (!job) return;

    if ($("detailsTitle")) $("detailsTitle").textContent = job.title;
    if ($("detailsRecruiter")) $("detailsRecruiter").textContent = `Posted by: ${job.recruiterName || "HireFlow Verified Employer"}`;
    if ($("detailsJobType")) $("detailsJobType").textContent = job.jobType || "Full Time";
    if ($("detailsLocation")) $("detailsLocation").textContent = `📍 ${job.location || "Remote"}`;
    const rawSal = (job.salary || "").trim();
    if (/\d/.test(rawSal)) {
        const hasPeriod = /\/(yr|mo|hr|month|year)|lpa|per\s+(year|month|hr)/i.test(rawSal);
        if ($("detailsSalary")) $("detailsSalary").textContent = hasPeriod ? rawSal : `${rawSal} / yr`;
    } else if (rawSal && rawSal.toLowerCase() !== "competitive") {
        if ($("detailsSalary")) $("detailsSalary").textContent = rawSal;
    } else {
        if ($("detailsSalary")) $("detailsSalary").textContent = "Competitive (Based on Experience)";
    }
    if ($("detailsApplicants")) $("detailsApplicants").textContent = `${job.applicantsCount || 0} Candidates Applied`;
    if ($("detailsDescription")) $("detailsDescription").textContent = job.description || "No description provided.";

    const applyBtn = $("detailsApplyBtn");
    if (applyBtn) {
        const user = getUserFromToken();
        if (user && user.role === "Recruiter") {
            applyBtn.textContent = "Recruiter View Only";
            applyBtn.disabled = true;
        } else {
            applyBtn.disabled = false;
            applyBtn.textContent = "Apply for this Position →";
            applyBtn.onclick = () => {
                closeModal("jobDetailsModal");
                handleApplyClick(job.id, job.title);
            };
        }
    }

    openModal("jobDetailsModal");
};

// =====================================================
// TRENDING TAGS CLICK HANDLER
// =====================================================
window.applyQuickTag = function (tag) {
    const searchInput = $("searchInput");
    if (searchInput) {
        searchInput.value = tag;
    }
    activeJobTypeFilter = "all";
    document.querySelectorAll(".filter-pill").forEach(p => {
        p.classList.toggle("active", p.dataset.type === "all");
    });
    renderFilteredJobs();

    const jobsSection = $("jobs");
    if (jobsSection) {
        jobsSection.scrollIntoView({ behavior: "smooth" });
    }
};

// =====================================================
// NAVBAR ROLE-AWARE RENDERING
// =====================================================
function updateNavbar() {
    const loggedIn = isLoggedIn();
    const loginBtn = $("loginBtn");
    const registerBtn = $("registerBtn");
    const postJobNavBtn = $("postJobNavBtn");
    const userBadgeLink = $("userBadgeLink");
    const logoutBtn = $("logoutBtn");
    const navUserAvatar = $("navUserAvatar");
    const navUserName = $("navUserName");
    const navUserRole = $("navUserRole");

    if (loggedIn) {
        const user = getUserFromToken();
        currentDecodedUser = user;

        if (loginBtn) loginBtn.classList.add("hidden");
        if (registerBtn) registerBtn.classList.add("hidden");

        if (userBadgeLink) userBadgeLink.classList.remove("hidden");
        if (logoutBtn) logoutBtn.classList.remove("hidden");

        if (user) {
            if (navUserName) navUserName.textContent = user.name;
            if (navUserRole) {
                navUserRole.textContent = user.role;
                if (user.role === "Recruiter") {
                    navUserRole.style.background = "#0891b2";
                } else {
                    navUserRole.style.background = "#4f46e5";
                }
            }
            if (navUserAvatar) {
                navUserAvatar.textContent = user.name.trim().charAt(0).toUpperCase() || "U";
            }

            // If Recruiter, show "+ Post a Job" button in navbar
            if (postJobNavBtn) {
                if (user.role === "Recruiter") {
                    postJobNavBtn.classList.remove("hidden");
                } else {
                    postJobNavBtn.classList.add("hidden");
                }
            }
        }
    } else {
        if (loginBtn) loginBtn.classList.remove("hidden");
        if (registerBtn) registerBtn.classList.remove("hidden");
        if (postJobNavBtn) postJobNavBtn.classList.add("hidden");
        if (userBadgeLink) userBadgeLink.classList.add("hidden");
        if (logoutBtn) logoutBtn.classList.add("hidden");
    }
}

// =====================================================
// JOB LISTINGS & FILTERING
// =====================================================
async function loadJobs() {
    const container = $("jobsContainer");
    if (!container) return;

    // Render smooth skeleton loading state
    container.innerHTML = `
        <div class="skeleton-card">
            <div style="display: flex; gap: 12px; align-items: center;">
                <div class="skeleton skeleton-avatar"></div>
                <div style="flex: 1;">
                    <div class="skeleton skeleton-title" style="margin-bottom: 6px;"></div>
                    <div class="skeleton skeleton-text" style="width: 40%;"></div>
                </div>
            </div>
            <div class="skeleton skeleton-text" style="margin-top: 10px;"></div>
            <div class="skeleton skeleton-text" style="width: 70%;"></div>
            <div style="display: flex; gap: 8px; margin-top: 12px;">
                <div class="skeleton skeleton-tag"></div>
                <div class="skeleton skeleton-tag"></div>
            </div>
        </div>
        <div class="skeleton-card">
            <div style="display: flex; gap: 12px; align-items: center;">
                <div class="skeleton skeleton-avatar"></div>
                <div style="flex: 1;">
                    <div class="skeleton skeleton-title" style="margin-bottom: 6px;"></div>
                    <div class="skeleton skeleton-text" style="width: 40%;"></div>
                </div>
            </div>
            <div class="skeleton skeleton-text" style="margin-top: 10px;"></div>
            <div class="skeleton skeleton-text" style="width: 70%;"></div>
            <div style="display: flex; gap: 8px; margin-top: 12px;">
                <div class="skeleton skeleton-tag"></div>
                <div class="skeleton skeleton-tag"></div>
            </div>
        </div>
        <div class="skeleton-card">
            <div style="display: flex; gap: 12px; align-items: center;">
                <div class="skeleton skeleton-avatar"></div>
                <div style="flex: 1;">
                    <div class="skeleton skeleton-title" style="margin-bottom: 6px;"></div>
                    <div class="skeleton skeleton-text" style="width: 40%;"></div>
                </div>
            </div>
            <div class="skeleton skeleton-text" style="margin-top: 10px;"></div>
            <div class="skeleton skeleton-text" style="width: 70%;"></div>
            <div style="display: flex; gap: 8px; margin-top: 12px;">
                <div class="skeleton skeleton-tag"></div>
                <div class="skeleton skeleton-tag"></div>
            </div>
        </div>
    `;

    try {
        const res = await fetch(`${API_BASE_URL}/Job`);
        if (!res.ok) throw new Error("Could not retrieve jobs");

        const jobs = await res.json();
        allJobsCache = Array.isArray(jobs) ? jobs : [];
        updateSavedJobsCount();
        renderFilteredJobs();
    } catch (err) {
        container.innerHTML = `
            <div class="empty-state" style="grid-column: 1 / -1;">
                <div class="empty-icon">⚠️</div>
                <h3>Unable to load jobs</h3>
                <p>${escapeHtml(err.message)}</p>
                <button type="button" class="btn btn-outline" onclick="loadJobs()">Try Again</button>
            </div>
        `;
    }
}

function renderFilteredJobs() {
    const container = $("jobsContainer");
    if (!container) return;

    const searchTerm = ($("searchInput")?.value || "").toLowerCase().trim();
    const locationTerm = ($("locationInput")?.value || "").toLowerCase().trim();
    const savedIds = getSavedJobIds();

    let filtered = allJobsCache.filter(job => {
        // Only show active jobs to candidates on home page
        if (!job.isActive) return false;

        // If saved-only filter active
        if (showingSavedOnly && !savedIds.includes(job.id)) {
            return false;
        }

        // Keyword filter
        const titleMatch = (job.title || "").toLowerCase().includes(searchTerm);
        const descMatch = (job.description || "").toLowerCase().includes(searchTerm);
        const recMatch = (job.recruiterName || "").toLowerCase().includes(searchTerm);
        const matchesKeyword = !searchTerm || titleMatch || descMatch || recMatch;

        // Location filter
        const locMatch = (job.location || "").toLowerCase().includes(locationTerm);
        const matchesLocation = !locationTerm || locMatch;

        // Job type filter
        const matchesType = activeJobTypeFilter === "all" ||
            (job.jobType || "").toLowerCase() === activeJobTypeFilter.toLowerCase();

        return matchesKeyword && matchesLocation && matchesType;
    });

    const countBadge = $("jobsHeadingCount");
    if (countBadge) {
        countBadge.textContent = filtered.length > 0 ? `${filtered.length} Available` : "0";
    }

    if (filtered.length === 0) {
        container.innerHTML = `
            <div class="empty-state" style="grid-column: 1 / -1;">
                <div class="empty-icon">${showingSavedOnly ? '💔' : '🔍'}</div>
                <h3>${showingSavedOnly ? 'No saved jobs found' : 'No jobs match your criteria'}</h3>
                <p>${showingSavedOnly ? 'Click the heart icon on any job card to save it for quick access!' : 'Try clearing your keyword filters or selecting "All Roles" above.'}</p>
                <button type="button" class="btn btn-primary" onclick="clearFilters()">Reset Filters</button>
            </div>
        `;
        return;
    }

    container.innerHTML = filtered.map(job => {
        const recruiterName = job.recruiterName || "Verified Recruiter";
        const recruiterInitial = recruiterName.trim().charAt(0).toUpperCase() || "H";
        const jobType = job.jobType || "Full Time";
        const isRemote = jobType.toLowerCase().includes("remote") || (job.location || "").toLowerCase().includes("remote");
        const badgeClass = isRemote ? "remote" : "full-time";
        const isSaved = savedIds.includes(job.id);

        const user = getUserFromToken();
        const isRecruiter = user && user.role === "Recruiter";

        // Intelligent Salary Formatting
        const rawSalary = (job.salary || "").trim();
        const isNumeric = /\d/.test(rawSalary);
        let salaryAmount = "Competitive";
        let salaryPeriod = "Based on experience";

        if (isNumeric) {
            salaryAmount = escapeHtml(rawSalary);
            const hasPeriod = /\/(yr|mo|hr|month|year)|lpa|per\s+(year|month|hr)/i.test(rawSalary);
            salaryPeriod = hasPeriod ? "" : "Per year";
        } else if (rawSalary && rawSalary.toLowerCase() !== "competitive") {
            salaryAmount = escapeHtml(rawSalary);
            salaryPeriod = "Market rate";
        }

        const safeJobTitle = escapeHtml(job.title);
        const safeTitleParam = safeJobTitle.replace(/'/g, "\\'");

        return `
            <div class="job-card" data-job-id="${job.id}">
                <div class="job-card-body">
                    <div class="job-card-header">
                        <div class="job-recruiter-info">
                            <div class="job-recruiter-avatar">${recruiterInitial}</div>
                            <div class="job-recruiter-details">
                                <h4>${escapeHtml(recruiterName)}</h4>
                                <span class="verified-tag">
                                    <svg width="12" height="12" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10 10-4.5 10-10S17.5 2 12 2zm-1.9 14.7l-4.4-4.4 1.4-1.4 3 3 7.4-7.4 1.4 1.4-8.8 8.8z"/></svg>
                                    Verified Employer
                                </span>
                            </div>
                        </div>
                        <span class="job-badge ${badgeClass}">${escapeHtml(jobType)}</span>
                    </div>

                    <h3 class="job-title" onclick="openJobDetails(${job.id})" title="Click to view details">
                        ${safeJobTitle}
                    </h3>
                    <p class="job-desc">${escapeHtml(job.description || "Exciting opportunity to build cutting-edge solutions.")}</p>

                    <div class="job-tags">
                        <span class="job-tag-chip">
                            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path><circle cx="12" cy="10" r="3"></circle></svg>
                            ${escapeHtml(job.location || 'Remote')}
                        </span>
                        <span class="job-tag-chip">
                            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg>
                            ${job.applicantsCount || 0} applied
                        </span>
                    </div>
                </div>

                <div class="job-card-footer">
                    <div class="job-salary-display">
                        <span class="job-salary-amount">${salaryAmount}</span>
                        ${salaryPeriod ? `<span class="job-salary-label">${salaryPeriod}</span>` : ''}
                    </div>

                    <div class="job-action-buttons">
                        <button type="button" class="bookmark-btn ${isSaved ? 'saved' : ''}" 
                            onclick="toggleSaveJob(event, ${job.id})" 
                            title="${isSaved ? 'Remove from Saved' : 'Save Job to Bookmarks'}">
                            <svg width="15" height="15" viewBox="0 0 24 24" fill="${isSaved ? '#f43f5e' : 'none'}" stroke="${isSaved ? '#f43f5e' : 'currentColor'}" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
                            </svg>
                        </button>

                        <button type="button" class="btn-icon-action" onclick="openJobDetails(${job.id})" title="Quick Overview / Details">
                            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                        </button>

                        ${isRecruiter ? `
                            <button type="button" class="btn btn-outline btn-sm" onclick="showToast('You are logged in as a Recruiter. Go to Dashboard to manage applicants.', 'info')">
                                Manage
                            </button>
                        ` : `
                            <button type="button" class="btn btn-primary btn-sm apply-btn" 
                                data-job-id="${job.id}" 
                                data-job-title="${safeJobTitle}"
                                onclick="handleApplyClick(${job.id}, '${safeTitleParam}')">
                                <span>Apply</span>
                                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="5" y1="12" x2="19" y2="12"></line><polyline points="12 5 19 12 12 19"></polyline></svg>
                            </button>
                        `}
                    </div>
                </div>
            </div>
        `;
    }).join("");
}

function clearFilters() {
    if ($("searchInput")) $("searchInput").value = "";
    if ($("locationInput")) $("locationInput").value = "";
    activeJobTypeFilter = "all";
    showingSavedOnly = false;

    const savedBtn = $("viewSavedJobsBtn");
    if (savedBtn) {
        savedBtn.classList.remove("btn-primary");
        savedBtn.classList.add("btn-outline");
        savedBtn.innerHTML = `❤️ Saved Jobs (<span id="savedJobsCount">${getSavedJobIds().length}</span>)`;
    }

    document.querySelectorAll(".filter-pill").forEach(p => {
        p.classList.toggle("active", p.dataset.type === "all");
    });

    renderFilteredJobs();
}

// =====================================================
// APPLY FLOW
// =====================================================
// =====================================================
// APPLY FLOW
// =====================================================
const APPLICANT_PROFILE_KEY = "hireflow_saved_applicant_profile";

function handleResumeFileSelect(e) {
    const file = e.target.files && e.target.files[0];
    if (!file) return;

    const resumeInput = $("appResume");
    const statusElem = $("appResumeFileStatus");
    const fileName = file.name || "";
    const fileExt = fileName.substring(fileName.lastIndexOf('.')).toLowerCase();
    const allowedExts = [".pdf", ".docx", ".doc"];

    // Condition: Only PDF and DOCX allowed
    if (!allowedExts.includes(fileExt)) {
        if (resumeInput) resumeInput.value = "";
        e.target.value = "";
        if (statusElem) {
            statusElem.innerHTML = `<span style="color: #ef4444; font-weight: 700;">❌ Only PDF and DOCX files are allowed! Kripya sirf .pdf ya .docx file chunein.</span>`;
        }
        showToast("Invalid file format! Only PDF and DOCX files are allowed.", "error");
        return;
    }

    const sizeKb = (file.size / 1024).toFixed(1);
    if (resumeInput) {
        resumeInput.value = file.name;
    }

    if (statusElem) {
        const badgeColor = fileExt === ".pdf" ? "#ef4444" : "#2563eb";
        statusElem.innerHTML = `<span style="color: #10b981; font-weight: 700;">✅ Selected</span> <span style="background: ${badgeColor}; color: #fff; padding: 2px 7px; border-radius: 4px; font-size: 11px; font-weight: 700;">${fileExt.toUpperCase().replace('.', '')}</span> ${escapeHtml(file.name)} (${sizeKb} KB)`;
    }
    showToast(`Resume attached (${fileExt.toUpperCase().replace('.', '')}): ${file.name}`, "success");
}

function handleApplyClick(jobId, jobTitle) {
    if (!isLoggedIn()) {
        openModal("loginModal");
        showToast("Please sign in as a candidate to submit your application.", "info");
        return;
    }

    const user = getUserFromToken();
    if (user && user.role === "Recruiter") {
        showToast("Recruiters cannot apply for jobs. Please switch to a Candidate account.", "error");
        return;
    }

    $("applicationJobId").value = jobId;
    $("applicationJobTitle").textContent = `Applying for: ${jobTitle}`;
    clearFormMessage("applicationMessage");

    // Load saved applicant profile from localStorage or user token
    let savedProfile = {};
    try {
        const raw = localStorage.getItem(APPLICANT_PROFILE_KEY);
        if (raw) savedProfile = JSON.parse(raw);
    } catch {}

    const nameParts = (user?.fullName || "").trim().split(/\s+/);
    const defaultFirst = nameParts[0] || "";
    const defaultLast = nameParts.length > 1 ? nameParts.slice(1).join(" ") : "";

    if ($("appFirstName")) $("appFirstName").value = savedProfile.firstName || defaultFirst;
    if ($("appMiddleName")) $("appMiddleName").value = savedProfile.middleName || "";
    if ($("appLastName")) $("appLastName").value = savedProfile.lastName || defaultLast;
    if ($("appEmail")) $("appEmail").value = savedProfile.email || user?.email || "";
    if ($("appMobile")) $("appMobile").value = savedProfile.mobile || "";
    if ($("appGender")) $("appGender").value = savedProfile.gender || "";
    if ($("appProfile")) $("appProfile").value = savedProfile.profile || "";
    if ($("appCurrentJob")) $("appCurrentJob").value = savedProfile.currentJob || "";
    if ($("appCurrentLocation")) $("appCurrentLocation").value = savedProfile.currentLocation || "";
    if ($("appHigherQualification")) $("appHigherQualification").value = savedProfile.qualification || "";
    if ($("appMarks10th")) $("appMarks10th").value = savedProfile.marks10th || "";
    if ($("appMarks12th")) $("appMarks12th").value = savedProfile.marks12th || "";
    if ($("appMarksUG")) $("appMarksUG").value = savedProfile.marksUG || "";
    if ($("appMarksPG")) $("appMarksPG").value = savedProfile.marksPG || "";
    if ($("appResume")) $("appResume").value = savedProfile.resume || "";
    if ($("coverLetter")) $("coverLetter").value = savedProfile.coverLetter || "";

    if ($("appResumeFileStatus")) $("appResumeFileStatus").textContent = "";

    openModal("applicationModal");
}

async function submitApplicationForm(e) {
    e.preventDefault();
    const jobId = $("applicationJobId").value;
    const msg = $("applicationMessage");

    const firstName = $("appFirstName")?.value.trim() || "";
    const middleName = $("appMiddleName")?.value.trim() || "";
    const lastName = $("appLastName")?.value.trim() || "";
    const email = $("appEmail")?.value.trim() || "";
    const mobile = $("appMobile")?.value.trim() || "";
    const gender = $("appGender")?.value.trim() || "";
    const profile = $("appProfile")?.value.trim() || "";
    const currentJob = $("appCurrentJob")?.value.trim() || "";
    const currentLocation = $("appCurrentLocation")?.value.trim() || "";
    const qualification = $("appHigherQualification")?.value.trim() || "";
    const marks10th = $("appMarks10th")?.value.trim() || "";
    const marks12th = $("appMarks12th")?.value.trim() || "";
    const marksUG = $("appMarksUG")?.value.trim() || "";
    const marksPG = $("appMarksPG")?.value.trim() || "";
    const resume = $("appResume")?.value.trim() || "";
    const coverLetter = $("coverLetter")?.value.trim() || "";

    // Validation
    if (!firstName || !lastName) {
        showFormMessage(msg, "Please provide your first and last name.", "error");
        return;
    }
    if (!email || !mobile) {
        showFormMessage(msg, "Please provide your contact email and mobile number.", "error");
        return;
    }
    if (!gender) {
        showFormMessage(msg, "Please select your gender.", "error");
        return;
    }
    if (!profile) {
        showFormMessage(msg, "Please select your profile/domain.", "error");
        return;
    }
    if (!currentJob) {
        showFormMessage(msg, "Please specify your current job or designation.", "error");
        return;
    }
    if (!currentLocation) {
        showFormMessage(msg, "Please specify your current location.", "error");
        $("appCurrentLocation")?.focus();
        return;
    }
    if (!qualification) {
        showFormMessage(msg, "Please select your highest qualification.", "error");
        return;
    }
    if (!marks10th || !marks12th || !marksUG) {
        showFormMessage(msg, "Please provide your 10th, 12th, and Under Graduation percentage marks.", "error");
        return;
    }
    if (!resume) {
        showFormMessage(msg, "Please select your Resume (PDF or DOCX).", "error");
        $("appResume")?.focus();
        return;
    }

    const lowerResume = resume.toLowerCase();
    const isValidDocFormat = lowerResume.endsWith('.pdf') || 
                             lowerResume.endsWith('.docx') || 
                             lowerResume.endsWith('.doc') || 
                             lowerResume.includes('.pdf') || 
                             lowerResume.includes('.docx');

    if (!isValidDocFormat) {
        showFormMessage(msg, "❌ Invalid Resume Format! Only PDF and DOCX files are allowed. Kripya PDF ya DOCX file select karein.", "error");
        if ($("appResumeFileStatus")) {
            $("appResumeFileStatus").innerHTML = `<span style="color: #ef4444; font-weight: 700;">❌ Only PDF and DOCX files are allowed (.pdf, .docx)</span>`;
        }
        $("appResume")?.focus();
        return;
    }

    if (!coverLetter) {
        showFormMessage(msg, "Please write a brief pitch or cover letter for the hiring manager.", "error");
        return;
    }

    // Persist applicant profile to localStorage for future 1-click applications
    try {
        localStorage.setItem(APPLICANT_PROFILE_KEY, JSON.stringify({
            firstName,
            middleName,
            lastName,
            email,
            mobile,
            gender,
            profile,
            currentJob,
            currentLocation,
            qualification,
            marks10th,
            marks12th,
            marksUG,
            marksPG,
            resume,
            coverLetter
        }));
    } catch {}

    const fullName = [firstName, middleName, lastName].filter(Boolean).join(" ");

    // Format structured application dossier for recruiter
    const applicationDossier = [
        `Candidate: ${fullName} (${gender})`,
        `Contact: ${email} | Mobile: ${mobile}`,
        `Current Location: ${currentLocation}`,
        `Profile Domain: ${profile} | Current Role: ${currentJob}`,
        `Academics: ${qualification}`,
        `Marks: 10th: ${marks10th}% | 12th: ${marks12th}% | UG: ${marksUG}%${marksPG ? ` | PG: ${marksPG}%` : ""}`,
        `Resume: ${resume}`,
        `\nPitch / Cover Letter:\n${coverLetter}`
    ].join("\n");

    const btn = $("submitApplicationBtn") || e.target.querySelector("button[type='submit']");
    btn.disabled = true;
    btn.textContent = "Submitting Application...";

    try {
        const payload = {
            firstName,
            middleName,
            lastName,
            email,
            mobile,
            gender,
            profileDomain: profile,
            currentJob,
            currentLocation,
            higherQualification: qualification,
            marks10th: marks10th ? parseFloat(marks10th) : null,
            marks12th: marks12th ? parseFloat(marks12th) : null,
            marksUG: marksUG ? parseFloat(marksUG) : null,
            marksPG: marksPG ? parseFloat(marksPG) : null,
            resume,
            coverLetter
        };

        const res = await fetch(`${API_BASE_URL}/Application/apply/${jobId}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                ...getAuthHeaders()
            },
            body: JSON.stringify(payload)
        });

        const data = await res.json().catch(() => ({}));

        if (!res.ok) {
            throw new Error(data.message || "Failed to submit application.");
        }

        closeModal("applicationModal");
        showToast(`🎉 Application for ${fullName} submitted successfully!`, "success");
        loadJobs(); // refresh applicant counts
    } catch (err) {
        showFormMessage(msg, err.message, "error");
    } finally {
        btn.disabled = false;
        btn.textContent = "Submit Application 🚀";
    }
}

// =====================================================
// RECRUITER POST JOB FLOW
// =====================================================
async function submitPostJobForm(e) {
    e.preventDefault();
    const title = $("postJobTitle").value.trim();
    const jobType = $("postJobType").value.trim();
    const location = $("postJobLocation").value.trim();
    const salary = $("postJobSalary").value.trim();
    const description = $("postJobDescription").value.trim();
    const msg = $("postJobMessage");

    const btn = $("submitPostJobBtn");
    btn.disabled = true;
    btn.textContent = "Publishing Position...";

    try {
        const res = await fetch(`${API_BASE_URL}/Job`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                ...getAuthHeaders()
            },
            body: JSON.stringify({ title, jobType, location, salary, description })
        });

        const data = await res.json().catch(() => ({}));

        if (!res.ok) {
            throw new Error(data.message || "Could not publish job.");
        }

        closeModal("postJobModal");
        showToast("✨ Job vacancy successfully published to HireFlow!", "success");
        e.target.reset();
        await loadJobs();
    } catch (err) {
        showFormMessage(msg, err.message, "error");
    } finally {
        btn.disabled = false;
        btn.textContent = "Publish Job Listing ✨";
    }
}

// =====================================================
// AUTHENTICATION: LOGIN & REGISTER
// =====================================================
async function handleLoginForm(e) {
    e.preventDefault();
    const email = $("loginEmail").value.trim();
    const password = $("loginPassword").value;
    const msg = $("loginMessage");
    const btn = e.target.querySelector("button[type='submit']");

    btn.disabled = true;
    btn.textContent = "Authenticating...";

    try {
        const res = await fetch(`${API_BASE_URL}/Auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password })
        });

        const data = await res.json().catch(() => ({}));

        if (!res.ok || !data.token) {
            throw new Error(data.message || "Invalid email or password.");
        }

        saveToken(data.token);
        closeModal("loginModal");
        updateNavbar();
        showToast("Login successful! Welcome back.", "success");
        loadJobs(); // re-render cards with role awareness
    } catch (err) {
        showFormMessage(msg, err.message, "error");
    } finally {
        btn.disabled = false;
        btn.textContent = "Sign In to Account";
    }
}

async function handleRegisterForm(e) {
    e.preventDefault();
    const fullName = $("registerFullName").value.trim();
    const email = $("registerEmail").value.trim();
    const password = $("registerPassword").value;
    const confirmPassword = $("registerConfirmPassword").value;
    const accountType = $("registerAccountType").value;
    const msg = $("registerMessage");
    const btn = e.target.querySelector("button[type='submit']");

    if (password !== confirmPassword) {
        showFormMessage(msg, "Passwords do not match.", "error");
        return;
    }

    btn.disabled = true;
    btn.textContent = "Creating Account...";

    try {
        const res = await fetch(`${API_BASE_URL}/Auth/register`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ fullName, email, password, confirmPassword, accountType })
        });

        const data = await res.json().catch(() => ({}));

        if (!res.ok) {
            throw new Error(data.message || "Registration failed.");
        }

        showFormMessage(msg, "Registration successful! Signing you in...", "success");

        // Automatically log in
        const loginRes = await fetch(`${API_BASE_URL}/Auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password })
        });

        const loginData = await loginRes.json().catch(() => ({}));

        if (loginRes.ok && loginData.token) {
            saveToken(loginData.token);
            closeModal("registerModal");
            updateNavbar();
            showToast(`Welcome to HireFlow, ${fullName}!`, "success");
            loadJobs();
        } else {
            closeModal("registerModal");
            openModal("loginModal");
            showToast("Account created! Please sign in.", "success");
        }
    } catch (err) {
        showFormMessage(msg, err.message, "error");
    } finally {
        btn.disabled = false;
        btn.textContent = "Complete Registration";
    }
}

function showFormMessage(el, text, type) {
    if (!el) return;
    el.className = `form-message ${type}`;
    el.textContent = text;
}

function clearFormMessage(el) {
    if (typeof el === "string") el = $(el);
    if (!el) return;
    el.className = "form-message";
    el.textContent = "";
}

// =====================================================
// BIND ALL EVENTS ON PAGE LOAD
// =====================================================
document.addEventListener("DOMContentLoaded", function () {
    // 1. Initial State & Themes
    initTheme();
    updateNavbar();
    loadJobs();

    // Navigation Menu Active Pill & Smooth Scroll Spy
    const navLinks = document.querySelectorAll(".nav-menu a");
    navLinks.forEach(link => {
        link.addEventListener("click", function () {
            navLinks.forEach(l => l.classList.remove("active"));
            this.classList.add("active");
        });
    });

    // Scroll spy to highlight active section
    const updateScrollSpy = () => {
        const scrollY = window.pageYOffset || document.documentElement.scrollTop;
        const windowHeight = window.innerHeight;
        const documentHeight = document.documentElement.scrollHeight;

        // 1. If user is at or near bottom of page, always highlight Contact
        if (scrollY + windowHeight >= documentHeight - 80) {
            navLinks.forEach(l => {
                const target = l.getAttribute("href")?.replace("#", "");
                l.classList.toggle("active", target === "contact");
            });
            return;
        }

        // 2. Section hierarchy from bottom to top
        const sectionTargets = [
            { id: "contact", nav: "contact" },
            { id: "about", nav: "features" },
            { id: "features", nav: "features" },
            { id: "jobs", nav: "jobs" },
            { id: "categories", nav: "categories" },
            { id: "home", nav: "home" }
        ];

        const triggerOffset = 200;
        for (const item of sectionTargets) {
            const el = document.getElementById(item.id);
            if (el) {
                const rect = el.getBoundingClientRect();
                if (rect.top <= triggerOffset && rect.bottom > 0) {
                    navLinks.forEach(l => {
                        const target = l.getAttribute("href")?.replace("#", "");
                        l.classList.toggle("active", target === item.nav);
                    });
                    break;
                }
            }
        }
    };

    window.addEventListener("scroll", updateScrollSpy, { passive: true });

    // 2. Modals Buttons
    $("loginBtn")?.addEventListener("click", () => {
        clearFormMessage("loginMessage");
        openModal("loginModal");
    });

    $("registerBtn")?.addEventListener("click", () => {
        clearFormMessage("registerMessage");
        openModal("registerModal");
    });

    $("heroRegisterBtn")?.addEventListener("click", () => {
        if (isLoggedIn()) {
            window.location.href = "/profile.html";
        } else {
            clearFormMessage("registerMessage");
            openModal("registerModal");
        }
    });

    $("ctaRegisterBtn")?.addEventListener("click", () => {
        if (isLoggedIn()) {
            window.location.href = "/profile.html";
        } else {
            clearFormMessage("registerMessage");
            openModal("registerModal");
        }
    });

    $("postJobNavBtn")?.addEventListener("click", () => {
        clearFormMessage("postJobMessage");
        openModal("postJobModal");
    });

    $("logoutBtn")?.addEventListener("click", () => {
        clearLoginSession();
        updateNavbar();
        showToast("Logged out successfully.", "info");
        loadJobs();
    });

    $("heroJobsBtn")?.addEventListener("click", () => {
        $("jobs")?.scrollIntoView({ behavior: "smooth" });
    });

    $("viewAllJobsBtn")?.addEventListener("click", () => {
        clearFilters();
        loadJobs();
        showToast("Job listings refreshed.", "info");
    });

    $("viewSavedJobsBtn")?.addEventListener("click", toggleViewSavedJobs);

    // Modal Close Buttons
    $("closeLoginBtn")?.addEventListener("click", () => closeModal("loginModal"));
    $("closeRegisterBtn")?.addEventListener("click", () => closeModal("registerModal"));
    $("closeApplicationBtn")?.addEventListener("click", () => closeModal("applicationModal"));
    $("closePostJobBtn")?.addEventListener("click", () => closeModal("postJobModal"));

    // Modal Switches
    $("switchToRegisterBtn")?.addEventListener("click", () => {
        closeModal("loginModal");
        clearFormMessage("registerMessage");
        openModal("registerModal");
    });

    $("switchToLoginBtn")?.addEventListener("click", () => {
        closeModal("registerModal");
        clearFormMessage("loginMessage");
        openModal("loginModal");
    });

    // Footer Links
    $("footerLoginBtn")?.addEventListener("click", (e) => {
        e.preventDefault();
        openModal("loginModal");
    });

    $("footerRegisterBtn")?.addEventListener("click", (e) => {
        e.preventDefault();
        openModal("registerModal");
    });

    // 3. Search & Real-time Live Filter with Debounce
    const debouncedFilter = () => {
        clearTimeout(searchDebounceTimer);
        searchDebounceTimer = setTimeout(renderFilteredJobs, 220);
    };

    $("searchBtn")?.addEventListener("click", renderFilteredJobs);
    $("searchInput")?.addEventListener("input", debouncedFilter);
    $("locationInput")?.addEventListener("input", debouncedFilter);
    $("searchInput")?.addEventListener("keyup", (e) => {
        if (e.key === "Enter") renderFilteredJobs();
    });
    $("locationInput")?.addEventListener("keyup", (e) => {
        if (e.key === "Enter") renderFilteredJobs();
    });

    // Filter Pills
    document.querySelectorAll(".filter-pill").forEach(pill => {
        pill.addEventListener("click", function () {
            document.querySelectorAll(".filter-pill").forEach(p => p.classList.remove("active"));
            this.classList.add("active");
            activeJobTypeFilter = this.dataset.type || "all";
            renderFilteredJobs();
        });
    });

    // 4. Form Submissions
    $("loginForm")?.addEventListener("submit", handleLoginForm);
    $("registerForm")?.addEventListener("submit", handleRegisterForm);
    $("applicationForm")?.addEventListener("submit", submitApplicationForm);
    $("postJobForm")?.addEventListener("submit", submitPostJobForm);

    // Resume interactive click & live input validation
    $("appResume")?.addEventListener("click", () => {
        if (!$("appResume").value.trim()) {
            $("appResumeFile")?.click();
        }
    });

    $("appResume")?.addEventListener("input", function() {
        const val = this.value.trim().toLowerCase();
        const statusElem = $("appResumeFileStatus");
        if (!val) {
            if (statusElem) statusElem.innerHTML = "";
            return;
        }
        const isValid = val.endsWith(".pdf") || val.endsWith(".docx") || val.endsWith(".doc");
        if (!isValid) {
            if (statusElem) {
                statusElem.innerHTML = `<span style="color: #ef4444; font-weight: 600;">⚠️ Only PDF and DOCX files are allowed (.pdf, .docx)</span>`;
            }
        } else {
            const ext = val.endsWith(".pdf") ? "PDF" : "DOCX";
            if (statusElem) {
                statusElem.innerHTML = `<span style="color: #10b981; font-weight: 600;">✅ Valid document: ${ext} format</span>`;
            }
        }
    });

    // 5. Password Visibility Toggles
    document.querySelectorAll(".password-toggle").forEach(btn => {
        btn.addEventListener("click", function () {
            const targetId = this.dataset.target;
            const input = $(targetId);
            if (!input) return;
            if (input.type === "password") {
                input.type = "text";
                this.textContent = "🙈";
            } else {
                input.type = "password";
                this.textContent = "👁️";
            }
        });
    });

    // 6. Escape Key & Outside Click to close Modals
    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape") {
            document.querySelectorAll(".modal.active").forEach(m => closeModal(m.id));
        }
    });

    document.querySelectorAll(".modal").forEach(modal => {
        modal.addEventListener("click", function (e) {
            if (e.target === this) {
                closeModal(this.id);
            }
        });
    });
});