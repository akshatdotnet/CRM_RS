$(document).ready(function () {
    // Auto-dismiss alerts
    setTimeout(function () { $('.alert').fadeOut('slow'); }, 4000);

    // Confirm delete
    $('[data-confirm]').on('click', function (e) {
        if (!confirm($(this).data('confirm'))) e.preventDefault();
    });

    // Stage update via AJAX
    $('.stage-btn').on('click', function () {
        var leadId = $(this).data('lead-id');
        var stage = $(this).data('stage');
        var token = $('input[name="__RequestVerificationToken"]').first().val();
        $.post('/Lead/UpdateStage', { id: leadId, stage: stage, __RequestVerificationToken: token })
            .done(function (res) {
                if (res.success) location.reload();
            });
    });

    // Search with debounce
    var searchTimer;
    $('#leadSearch').on('input', function () {
        clearTimeout(searchTimer);
        var q = $(this).val();
        searchTimer = setTimeout(function () {
            if (q.length >= 2 || q.length === 0) {
                window.location = '/Lead?q=' + encodeURIComponent(q);
            }
        }, 400);
    });
});
