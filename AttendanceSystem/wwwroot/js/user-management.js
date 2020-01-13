var startDate = toDate('#StartDate');
var endDate = toDate('#EndDate');

$('#StartDate').on('change', e => {
    startDate = toDate('#StartDate')
    updateTotalDays(dateDifference(startDate, endDate))
    $('#EndDate').attr('min', $('#StartDate').val());
    //console.log({ startDate, endDate })
    //console.log(dateDifference(startDate, endDate))
});

$('#EndDate').on('change', e => {
    endDate = toDate('#EndDate')
    updateTotalDays(dateDifference(startDate, endDate))
    $('#StartDate').attr('max', $('#EndDate').val());
    //console.log({ startDate, endDate })
    //console.log(dateDifference(startDate, endDate))
});

function toDate(selector) {
    const [year, month, day] = $(selector).val().split("-")
    return new Date(year, month - 1, day)
}

function updateTotalDays(totalDays) {
    if (totalDays <= 0) {
        $('#TotalDays').val(0);
        $("[data-valmsg-for='TotalDays']").text("Make sure End Date is after Start Date, and not all selected vacation days are on a weekend!");
        $('input[type="submit"]').attr('disabled', true)
    }

    else if (!isNaN(totalDays)) {
        $('#TotalDays').val(totalDays);
        $("[data-valmsg-for='TotalDays']").text('');
        $('input[type="submit"]').attr('disabled', false)
    }
}

function dateDifference(start, end) {

    // Copy date objects so we don't modify originals
    var s = new Date(+start);
    var e = new Date(+end);

    var days = 0;
        while (s <= e) {
            if (s.getDay() != 5 && s.getDay() != 6) {
                ++days;
            }

            s.setDate(s.getDate() + 1);
        }
    return days;
}