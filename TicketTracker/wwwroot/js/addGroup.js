$(document).ready(function () {
    $("#addGroup").click(addGroup)
})

var addGroup = function () {
    $("#groupList").append("<li class='list-group-item'><label>Group</label><input id='groups' name='groups' class='form-control' /></li>")
}