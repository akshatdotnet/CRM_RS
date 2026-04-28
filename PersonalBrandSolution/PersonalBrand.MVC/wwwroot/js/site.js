// ─────────────────────────────────────────────────────
//  PersonalBrand MVC — site.js
//  Consumes real API data via server-rendered HTML +
//  AJAX calls for interactive features
// ─────────────────────────────────────────────────────

$(function () {
  initNavbar();
  initHeroTyping();
  initCounters();
  initSkillBars();
  initScrollAnimations();
  initQAFilters();
  initPipelineModal();
  initCookieBanner();
  initContactForm();
  initBackToTop();
  showServerToast();
});

// ── NAVBAR ─────────────────────────────────────────────
function initNavbar() {
  $(window).on('scroll', function () {
    $('#mainNav').toggleClass('scrolled', $(this).scrollTop() > 60);
  });
  $(window).on('scroll', function () {
    const pos = $(this).scrollTop() + 100;
    $('section[id]').each(function () {
      const top = $(this).offset().top;
      const bot = top + $(this).outerHeight();
      const id = $(this).attr('id');
      if (pos >= top && pos <= bot) {
        $('.navbar-nav .nav-link').removeClass('active').css('color', '');
        $(`.navbar-nav .nav-link[href="#${id}"]`).addClass('active').css('color', 'var(--accent-cyan)');
      }
    });
  });
}

// ── HERO TYPING ────────────────────────────────────────
function initHeroTyping() {
  const roles = [
    'Senior .NET Core Developer',
    'Azure Cloud Architect',
    'IT Consultant & Trainer',
    'Microservices Expert',
    'Open Source Contributor'
  ];
  let ri = 0, ci = 0, deleting = false;
  const el = document.getElementById('typingText');
  if (!el) return;
  function type() {
    const word = roles[ri];
    el.textContent = deleting ? word.substring(0, --ci) : word.substring(0, ++ci);
    if (!deleting && ci === word.length) { deleting = true; setTimeout(type, 1800); return; }
    if (deleting && ci === 0) { deleting = false; ri = (ri + 1) % roles.length; }
    setTimeout(type, deleting ? 60 : 90);
  }
  setTimeout(type, 600);
}

// ── COUNTERS ───────────────────────────────────────────
function initCounters() {
  const observer = new IntersectionObserver(function (entries) {
    entries.forEach(function (e) {
      if (!e.isIntersecting) return;
      const el = $(e.target);
      const target = parseInt(el.data('target')) || 0;
      const suffix = el.closest('.hero-stat-num').text().includes('+') || target > 100 ? '+' : '';
      let current = 0;
      const step = Math.max(1, Math.ceil(target / 60));
      const timer = setInterval(function () {
        current = Math.min(current + step, target);
        el.text(current >= 1000 ? (current / 1000).toFixed(1) + 'K+' : current + suffix);
        if (current >= target) clearInterval(timer);
      }, 30);
      observer.unobserve(e.target);
    });
  }, { threshold: 0.5 });
  document.querySelectorAll('.counter').forEach(el => observer.observe(el));
}

// ── SKILL BARS ─────────────────────────────────────────
function initSkillBars() {
  const observer = new IntersectionObserver(function (entries) {
    entries.forEach(function (e) {
      if (!e.isIntersecting) return;
      $(e.target).find('.skill-bar-fill').each(function () {
        $(this).css('width', $(this).data('pct') + '%');
      });
      observer.unobserve(e.target);
    });
  }, { threshold: 0.3 });
  const ab = document.getElementById('about');
  if (ab) observer.observe(ab);
}

// ── SCROLL ANIMATIONS ──────────────────────────────────
function initScrollAnimations() {
  const observer = new IntersectionObserver(function (entries) {
    entries.forEach(function (e) {
      if (e.isIntersecting) e.target.classList.add('visible');
    });
  }, { threshold: 0.08 });
  document.querySelectorAll('.fade-in-up').forEach(el => observer.observe(el));
}

// ── Q&A FILTERS (AJAX) ─────────────────────────────────
function initQAFilters() {
  let searchTimer;

  // Client-side filter using server-rendered data
  function filterQA(level, search) {
    const items = window.AppData?.qaItems || [];
    let filtered = items;
    if (level && level !== 'all') filtered = filtered.filter(q => q.level === level);
    if (search) {
      const term = search.toLowerCase();
      filtered = filtered.filter(q =>
        q.question.toLowerCase().includes(term) ||
        q.answer.toLowerCase().includes(term) ||
        q.category.toLowerCase().includes(term));
    }
    renderQAItems(filtered);
  }

  // Filter button click
  $('#qaFilters .qa-filter-btn').on('click', function () {
    $('#qaFilters .qa-filter-btn').removeClass('active');
    $(this).addClass('active');
    filterQA($(this).data('cat'), $('#qaSearch').val());
  });

  // Search
  $('#qaSearch').on('input', function () {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => {
      const level = $('#qaFilters .qa-filter-btn.active').data('cat');
      filterQA(level, $(this).val());
    }, 300);
  });

  // Initial server-rendered accordion is shown; when filter changes, hide it and show rendered results
  function renderQAItems(items) {
    const orig = $('#qaAccordion');
    const ajax = $('#qaAjaxResults');
    if (!items || items.length === 0) {
      orig.hide();
      ajax.html('<p class="text-center" style="color:var(--text-muted);font-family:var(--font-mono);font-size:0.85rem;padding:32px 0">No results found</p>');
      return;
    }
    const level = $('#qaFilters .qa-filter-btn.active').data('cat');
    const search = $('#qaSearch').val();
    if (!level || level === 'all') { orig.show(); ajax.empty(); return; }
    orig.hide();
    let html = '';
    items.forEach(function (item) {
      html += `
        <div class="accordion-item qa-accordion">
          <h2 class="accordion-header">
            <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#qar${item.id}">
              <div class="qa-btn-wrap">
                <span>${escHtml(item.question)}</span>
                <span class="qa-level ${item.level}">${item.level.toUpperCase()}</span>
                <span class="ms-2" style="font-family:var(--font-mono);font-size:0.65rem;color:var(--text-muted);flex-shrink:0">${escHtml(item.category)}</span>
              </div>
            </button>
          </h2>
          <div id="qar${item.id}" class="accordion-collapse collapse">
            <div class="accordion-body">${escHtml(item.answer)}</div>
          </div>
        </div>`;
    });
    ajax.html(html);
  }
}

// ── PIPELINE LEAD MODAL ────────────────────────────────
function initPipelineModal() {
  const statusLabels = { new:'Lead', contacted:'Contacted', proposal:'Proposal', negotiation:'Negotiation', closed:'Closed ✓' };
  const emailTemplates = {
    new: "Hi [Name], I saw your inquiry about [topic]. I'd love to set up a quick call. Are you available this week?",
    contacted: "Hi [Name], Following up on our conversation. I've put together some initial thoughts — happy to share. When works for you?",
    proposal: "Hi [Name], I've prepared a detailed proposal for your review. Let me know if you have any questions.",
    negotiation: "Hi [Name], Thank you for the feedback. I'm flexible on the terms — let's schedule a call to finalize details.",
    closed: "Hi [Name], Wonderful working with you! I'll send the contract and kickoff details shortly."
  };

  let currentLeadId = null;
  let currentStatus = null;

  window.openLeadModal = function (id, name, status, role, value, service) {
    currentLeadId = id;
    currentStatus = status;

    const statusOpts = Object.keys(statusLabels).map(s =>
      `<option value="${s}" ${s === status ? 'selected' : ''}>${statusLabels[s]}</option>`
    ).join('');

    $('#leadModalLabel').text(name);
    $('#leadModalBody').html(`
      <div style="display:flex;gap:12px;flex-wrap:wrap;margin-bottom:12px">
        <span style="font-family:var(--font-mono);font-size:0.75rem;color:var(--text-muted)"><i class="bi bi-briefcase me-1"></i>${escHtml(role)}</span>
        <span style="font-family:var(--font-mono);font-size:0.75rem;color:var(--accent-cyan)"><i class="bi bi-currency-rupee me-1"></i>${escHtml(value)}</span>
        <span style="font-family:var(--font-mono);font-size:0.75rem;color:var(--text-muted)"><i class="bi bi-tools me-1"></i>${escHtml(service)}</span>
      </div>
      <div class="form-group-custom" style="margin-bottom:16px">
        <label class="form-label-custom">Update Status</label>
        <select class="form-control-custom" id="leadStatusSelect" style="max-width:220px">${statusOpts}</select>
      </div>
      <div style="margin-bottom:16px">
        <label class="form-label-custom">Email Follow-up Template</label>
        <div id="emailTemplate" style="background:var(--bg-secondary);border:1px solid var(--border);border-radius:8px;padding:14px;font-size:0.82rem;color:var(--text-secondary);font-family:var(--font-mono);line-height:1.7">${emailTemplates[status] || emailTemplates.new}</div>
        <button class="btn-outline-custom mt-2" style="font-size:0.72rem;padding:8px 16px" onclick="copyEmailTemplate()">
          <i class="bi bi-clipboard"></i> Copy Template
        </button>
      </div>
      <div>
        <label class="form-label-custom">Add Note</label>
        <div style="display:flex;gap:8px">
          <input type="text" class="form-control-custom" id="newLeadNote" placeholder="Add a note about this lead..." style="flex:1">
          <button class="btn-primary-custom" style="padding:10px 16px;font-size:0.75rem" onclick="submitLeadNote()">
            <span><i class="bi bi-plus"></i></span>
          </button>
        </div>
      </div>
    `);

    // Status change updates template
    $('#leadStatusSelect').on('change', function () {
      currentStatus = $(this).val();
      $('#emailTemplate').text(emailTemplates[currentStatus] || emailTemplates.new);
    });

    new bootstrap.Modal(document.getElementById('leadModal')).show();
  };

  // Save lead changes
  $('#saveLeadBtn').on('click', async function () {
    if (!currentLeadId || !currentStatus) return;
    try {
      const resp = await $.ajax({
        url: '/Home/UpdateLeadStatus',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ leadId: currentLeadId, status: currentStatus }),
        headers: { 'RequestVerificationToken': getAntiForgeryToken() }
      });
      if (resp.success) {
        showToast('Lead updated successfully!');
        bootstrap.Modal.getInstance(document.getElementById('leadModal')).hide();
        // Refresh pipeline stats from API
        refreshPipeline();
      } else {
        showToast('Update failed — please try again', 'error');
      }
    } catch {
      showToast('Network error — please try again', 'error');
    }
  });

  window.submitLeadNote = async function () {
    const note = $('#newLeadNote').val().trim();
    if (!note || !currentLeadId) return;
    try {
      const resp = await $.ajax({
        url: '/Home/AddLeadNote',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ leadId: currentLeadId, note }),
        headers: { 'RequestVerificationToken': getAntiForgeryToken() }
      });
      if (resp.success) {
        $('#newLeadNote').val('');
        showToast('Note added!');
      }
    } catch { showToast('Failed to add note', 'error'); }
  };

  window.copyEmailTemplate = function () {
    const text = $('#emailTemplate').text();
    navigator.clipboard.writeText(text).then(() => showToast('Email template copied!'));
  };
}

// ── PIPELINE REFRESH (AJAX) ────────────────────────────
async function refreshPipeline() {
  try {
    const resp = await $.getJSON('/Home/Pipeline');
    if (!resp.success || !resp.data) return;
    const data = resp.data;
    // Update stat numbers
    const labels = { new:'Lead', contacted:'Contacted', proposal:'Proposal', negotiation:'Negotiation', closed:'Closed ✓' };
    Object.keys(labels).forEach(function (s) {
      const cnt = (data.countByStatus && data.countByStatus[s]) || 0;
      $(`.pipeline-stat-num`).filter(function () {
        return $(this).next('.pipeline-stat-lbl').text().trim() === labels[s];
      }).text(cnt);
    });
  } catch (e) {
    console.warn('Pipeline refresh failed', e);
  }
}

// ── CONTACT FORM ───────────────────────────────────────
function initContactForm() {
  $('#contactForm').on('submit', function () {
    const btn = $(this).find('button[type="submit"]');
    btn.prop('disabled', true);
    $('#submitText').html('<i class="bi bi-hourglass-split me-2"></i>Sending...');
  });
}

// ── BACK TO TOP ────────────────────────────────────────
function initBackToTop() {
  $(window).on('scroll', function () {
    const btn = $('#backToTop');
    if ($(this).scrollTop() > 400) {
      btn.css({ opacity: 1, 'pointer-events': 'auto' });
    } else {
      btn.css({ opacity: 0, 'pointer-events': 'none' });
    }
  });
}

// ── COOKIE BANNER ──────────────────────────────────────
function initCookieBanner() {
  if (!localStorage.getItem('cookieAccepted')) {
    setTimeout(() => $('#cookieBanner').fadeIn(), 2000);
  }
  $('#acceptCookies').on('click', function () {
    localStorage.setItem('cookieAccepted', '1');
    $('#cookieBanner').fadeOut();
  });
  $('#declineCookies').on('click', function () { $('#cookieBanner').fadeOut(); });
}

// ── SERVER TOAST (from TempData) ──────────────────────
function showServerToast() {
  const el = document.getElementById('serverToast');
  if (!el) return;
  const msg = el.dataset.msg;
  const type = el.dataset.type;
  if (msg) setTimeout(() => showToast(msg, type), 500);
}

// ── TOAST ──────────────────────────────────────────────
function showToast(msg, type) {
  const border = type === 'error' ? 'var(--danger)' : type === 'info' ? 'var(--warning)' : 'var(--accent-cyan)';
  const toast = $(`
    <div style="position:fixed;bottom:32px;right:32px;z-index:99999;
      background:var(--bg-card);border:1px solid ${border};border-left:4px solid ${border};
      color:var(--text-primary);font-family:var(--font-mono);font-size:0.82rem;
      padding:14px 20px;border-radius:8px;max-width:380px;
      box-shadow:0 8px 32px rgba(0,0,0,0.4);">
      ${type === 'error' ? '❌ ' : type === 'info' ? 'ℹ️ ' : '✅ '}${escHtml(msg)}
    </div>`);
  $('body').append(toast);
  setTimeout(() => toast.fadeOut(400, function () { $(this).remove(); }), 4500);
}

// ── HELPERS ────────────────────────────────────────────
function escHtml(str) {
  if (!str) return '';
  return String(str)
    .replace(/&/g, '&amp;').replace(/</g, '&lt;')
    .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function getAntiForgeryToken() {
  return $('input[name="__RequestVerificationToken"]').first().val() || '';
}
