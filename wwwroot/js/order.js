
var dataTable;

$(document).ready(function () {

    var url = window.location.search;

    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else if (url.includes("completed")) {
        loadDataTable("completed");
    }
    else if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else if (url.includes("approved")) {
        loadDataTable("approved");
    }
    else {
        loadDataTable();
    }
        
});

function loadDataTable(status) {
    dataTable = $('#order-list-table').DataTable(
        {
            "ajax": { url: '/Admin/Order/getall?status=' + status },

            "columns":
                [
                    { data: 'id', "width": "auto" },
                    { data: 'applicationUser.name', "width": "auto" },
                    { data: 'applicationUser.phoneNumber', "width": "auto" },
                    { data: 'applicationUser.email', "width": "auto" },
                    { data: 'orderStatus', "width": "auto" },
                    { data: 'orderTotal', "width": "auto" },
                    {
                        data: 'id',
                        "render": function (data) {
                            return `<div class="btn-group" role="group">
                                    <a href="/admin/Order/details?orderId=${data}" class="btn btn-outline-primary mx-2">
                                        <i class="bi bi-pencil-square"></i> Details
                                    </a>
                                </div > `
                        },
                        "width": "auto"
                    }
                ]

        }
    );
}



