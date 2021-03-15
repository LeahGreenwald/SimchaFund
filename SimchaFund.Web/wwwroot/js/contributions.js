$(() => {
    $(function () {
        $(".contribute").change();
    });
    $(".contribute").change(function () {
        let checked = $(this).prop('checked');
        if (checked) {
            $(this).val(true);
        }
        else {
            $(this).val(false);
        }
    })
})