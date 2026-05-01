$(function () {
    // Sidebar toggle
    $('#sidebarToggle').on('click', function () {
        var $sidebar = $('#sidebar');
        if ($(window).width() <= 768) {
            $sidebar.toggleClass('mobile-open');
        } else {
            $sidebar.toggleClass('collapsed');
            localStorage.setItem('sidebarCollapsed', $sidebar.hasClass('collapsed'));
        }
    });

    // Restore sidebar state
    if (localStorage.getItem('sidebarCollapsed') === 'true') {
        $('#sidebar').addClass('collapsed');
    }

    // Auto-dismiss alerts after 4s
    setTimeout(function () {
        $('.alert.alert-success, .alert.alert-danger').fadeOut(400);
    }, 4000);

    // Confirm delete via data attribute
    $(document).on('submit', 'form[data-confirm]', function (e) {
        var msg = $(this).data('confirm') || 'Are you sure?';
        if (!confirm(msg)) { e.preventDefault(); }
    });

    // AJAX toggle active (optional enhancement)
    $(document).on('click', '.js-toggle-active', function (e) {
        e.preventDefault();
        var $btn = $(this);
        var url = $btn.data('url');
        $.post(url, { __RequestVerificationToken: $('input[name=__RequestVerificationToken]').first().val() })
            .done(function () { location.reload(); })
            .fail(function () { alert('Action failed.'); });
    });
});
