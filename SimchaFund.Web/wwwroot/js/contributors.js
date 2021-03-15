$(() => {
    $("#new-contributor").on('click', function () {
        $("#contributor-modal").modal();
    });

    $("tbody").on('click', '#deposit', function () {
        const button = $(this);
        let id = $(button).data("deposit-id");
        let name = $(button).data("deposit-name");
        $("#deposit-name").val(name);
        $("#contributorId").val(id)
        $("#deposit-modal").modal();
    });

    $("#search").on('change', function () {

    });

    $("#clear-search").on('click', function () {
        $("#search").val('');
    });
    $("tbody").on('click', "#edit-contributor", function () {
        let button = $(this);
        let firstName = $(button).data("first-name")
        $("#contributor_first_name").attr('value', firstName);
        let lastName = $(button).data("last-name")
        $("#contributor_last_name").attr('value', lastName);
        let cell = $(button).data("cell")
        $("#contributor_cell").attr('value', cell);
        let alwaysInclude = $(button).data("always-include")
        $("#contributor_always_include").attr('value', alwaysInclude);
        $("#initialDepositDiv").remove();        

        $("#contributor-modal").modal();
    });
});