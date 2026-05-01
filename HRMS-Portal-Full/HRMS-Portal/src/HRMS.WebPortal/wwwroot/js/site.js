// HRMS Portal — site.js
// Handles: quick-login fill, confirm dialogs, search form auto-submit

document.addEventListener('DOMContentLoaded', function () {

    // ── Quick Login credential fill ────────────────────────────────────────
    document.querySelectorAll('.cred-fill').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var email    = btn.dataset.email;
            var password = btn.dataset.password;
            var emailEl  = document.getElementById('Email');
            var passEl   = document.getElementById('Password');
            if (emailEl) emailEl.value = email;
            if (passEl)  passEl.value  = password;
            // Visual feedback
            btn.classList.add('cred-fill--active');
            setTimeout(function () { btn.classList.remove('cred-fill--active'); }, 600);
        });
    });

    // ── Auto-submit search form on filter change ───────────────────────────
    document.querySelectorAll('.filter-auto-submit').forEach(function (el) {
        el.addEventListener('change', function () {
            el.closest('form').submit();
        });
    });

    // ── Search debounce (300ms) ────────────────────────────────────────────
    var searchInput = document.querySelector('.search-debounce');
    if (searchInput) {
        var debounceTimer;
        searchInput.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(function () {
                searchInput.closest('form').submit();
            }, 400);
        });
    }

    // ── Salary slip preview calc ───────────────────────────────────────────
    var grossInput = document.getElementById('GrossMonthly');
    if (grossInput) {
        grossInput.addEventListener('input', updateSalaryPreview);
        updateSalaryPreview();
    }

    function updateSalaryPreview() {
        var gross      = parseFloat(document.getElementById('GrossMonthly')?.value) || 0;
        var basicPct   = parseFloat(document.getElementById('BasicPercent')?.value) || 40;
        var hraPct     = parseFloat(document.getElementById('HRAPercent')?.value)   || 20;
        var splPct     = parseFloat(document.getElementById('SpecialAllowancePercent')?.value) || 30;
        var pt         = parseFloat(document.getElementById('ProfessionalTax')?.value) || 200;

        var basic = gross * basicPct / 100;
        var hra   = gross * hraPct   / 100;
        var spl   = gross * splPct   / 100;
        var pf    = basic * 0.12;
        var net   = gross - pf - pt;

        setPreview('prev-basic',  basic);
        setPreview('prev-hra',    hra);
        setPreview('prev-spl',    spl);
        setPreview('prev-pf',     pf);
        setPreview('prev-pt',     pt);
        setPreview('prev-net',    net);
        setPreview('prev-ctc',    (gross + pf) * 12);
    }

    function setPreview(id, val) {
        var el = document.getElementById(id);
        if (el) el.textContent = 'Rs. ' + val.toLocaleString('en-IN', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    }

    // ── Percent fields: recalc on change ──────────────────────────────────
    ['BasicPercent','HRAPercent','SpecialAllowancePercent','ProfessionalTax'].forEach(function (id) {
        var el = document.getElementById(id);
        if (el) el.addEventListener('input', updateSalaryPreview);
    });

    // ── Form validation helper: highlight empty required fields ───────────
    document.querySelectorAll('form.validate-required').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            var valid = true;
            form.querySelectorAll('[required]').forEach(function (field) {
                if (!field.value.trim()) {
                    field.style.borderColor = 'var(--red)';
                    valid = false;
                } else {
                    field.style.borderColor = '';
                }
            });
            if (!valid) e.preventDefault();
        });
    });

});
