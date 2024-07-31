﻿
var dataTable;

$(document).ready(function ()
{
    loadDataTable();
});

function loadDataTable()
{
    dataTable = $('#product-table').DataTable(
        {
            "ajax": { url : '/admin/Product/getall'},

            "columns":
             [
                { data: 'name', "width" : "20%" },
                { data: 'version', "width" : "5%" },
                { data: 'year', "width" : "5%" },
                { data: 'price', "width": "10%" },
                { data: 'manufacturer', "width": "10%" },
                { data: 'category.name', "width": "10%" },
                {
                    data: 'id',
                    "render": function (data)
                    {
                        return `<div class="btn-group" role="group">
                                    <a href="/admin/Product/upsert?id=${data}" class="btn btn-outline-primary mx-2">
                                        <i class="bi bi-pencil-square"></i> Edit
                                    </a>
                                    <a onClick=Delete('/admin/Product/delete/${data}') class="btn btn-outline-danger mx-2">
                                       <i class="bi bi-trash-fill"></i> Delete
                                    </a>
                                </div > `
                    },
                    "width": "20%"
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

