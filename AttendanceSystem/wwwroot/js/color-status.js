
$(document).bind('DOMSubtreeModified', () => {
    $('td').each(function () {
        if ($(this).text() == 'Pending') {
            $(this).addClass('text-warning')
        }
        else if ($(this).text() == 'Rejected') {
            $(this).addClass('text-red')
        }
        else if ($(this).text() == 'Approved') {
            $(this).addClass('text-green')
        }
    });
});