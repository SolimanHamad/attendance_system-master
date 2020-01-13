// Please see documentation at https://docs.microsoft.com/aspnet/core/client/side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    $('input[daterangepicker]').each(function () {
        var isRange = $(this).data("comparison") === 'Range';
        var limitInMinutes = $(this).data('limit_in_minutes');

        var options = {
            autoUpdateInput: false,
            timePicker: false,
            singleDatePicker: !isRange
        };

        if (limitInMinutes > 0)
            options.dateLimit = {
                'minutes': limitInMinutes
            };
        $(this).daterangepicker(options, function () {
            if (isRange)
                this.element.val(this.startDate.format("YYYY/MM/DD") + " - " + this.endDate.format("YYYY/MM/DD"));
            else
                this.element.val(this.startDate.format("YYYY/MM/DD"));
        });
    });
});

$.ajax({
    type: 'GET',
    url: '/api/Notification',
    success: function (res) {
        res.vacationsRequests > 0 ? $("#vacations").text(res.vacationsRequests) : $("#vacations").text('');
        res.missedEvents > 0 ? $("#missed").text(res.missedEvents) : $("#missed").text('');
        res.editEvents > 0 ? $("#edited").text(res.editEvents) : $("#edited").text('');
        res.offensesApproval > 0 ? $("#offenses").text(res.offensesApproval) : $("#offenses").text('');
    }
});