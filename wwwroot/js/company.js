
var dataTable;

$(document).ready(function ()
{
    loadDataTable();
});

function loadDataTable()
{
    dataTable = $('#company-table').DataTable(
        {
            "ajax": { url : '/admin/Company/getall'},

            "columns":
             [
                { data: 'name', "width" : "20%" },
                { data: 'phoneNumber', "width" : "auto" },
                { data: 'streetAddress', "width" : "auto" },
                { data: 'city', "width": "auto" },
                { data: 'division', "width": "auto" },
                { data: 'postalCode', "width": "auto" },
                {
                    data: 'id',
                    "render": function (data)
                    {
                        return `<div class="btn-group" role="group">
                                    <a href="/admin/Company/upsert?id=${data}" class="btn btn-outline-primary mx-2">
                                        <i class="bi bi-pencil-square"></i> Edit
                                    </a>
                                    <a onClick=Delete('/admin/Company/delete/${data}') class="btn btn-outline-danger mx-2">
                                       <i class="bi bi-trash-fill"></i> Delete
                                    </a>
                                </div > `
                    },
                    "width": "auto"
                }
             ]

        }
    );
}

function Delete(url)
{
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax(
                {
                    url: url,
                    type: 'DELETE',
                    success: function (data)
                    {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                }
            )
        }
    });
}

