/// Simple frontend for Revo.Examples.Todos example app
/// NOTE This is just a quick and dirty implementation meant only as a simple way to interact
/// with the APIs exposed by the sample application. Itself doesn't actually showcase any features of Revo.

function newGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function addTodoList() {
    $("#addTodoListForm").prop("disabled", true);

    $.ajax("/api/todo-lists",
        {
            data: JSON.stringify({ id: newGuid(), name: $("#addTodoListName")[0].value }),
            contentType: 'application/json',
            type: 'POST'
        }).done(function () {
            reloadTodoLists();
            $("#addTodoListForm")[0].reset();
            $("#addTodoListForm").prop("disabled", false);
        });
}

function addTodo(todoList, addTodoInput, addTodoButton) {
    if (!addTodoInput[0].value) {
        return;
    }

    addTodoInput.prop('disabled', true);
    addTodoButton.prop('disabled', true);

    $.ajax("/api/todo-lists/" + todoList.id,
        {
            data: JSON.stringify({ text: addTodoInput[0].value }),
            contentType: 'application/json',
            type: 'POST'
        }).done(function () {
            reloadTodoLists();
            addTodoInput.prop('disabled', false);
            addTodoButton.prop('disabled', false);
    });
}

function invertTodoChecked(todo) {
    $.ajax("/api/todo-lists/" + todo.todoListId + "/" + todo.id,
        {
            data: JSON.stringify({ text: todo.text, isComplete: !todo.isComplete }),
            contentType: 'application/json',
            type: 'PUT'
        }).done(function () {
            reloadTodoLists();
    });
}

function reloadTodoLists() {
    $("#todoLists").css("display", "none");
    $("#todoListsLoader").css("display", "block");

    $.get("/api/todo-lists",
        function (data) {
            $("#todoLists").empty();

            for (let todoList of data) {
                let div = $('<div/>')
                    .addClass('todoList')
                    .appendTo($("#todoLists"));
                let h3 = $('<h3/>')
                    .text(todoList.name)
                    .appendTo(div);
                
                let div2 = $('<div/>')
                    .appendTo(div);

                let addTodoInput = $('<input/>')
                    .prop('type', 'text')
                    //.attr('required', true)
                    .prop('placeholder', 'Add task...')
                    .appendTo(div2);
                let addTodoButton = $('<button/>')
                    .text('+')
                    .appendTo(div2)
                    .click(function() {
                        addTodo(todoList, addTodoInput, addTodoButton);
                    });
                
                let ul = $('<ul/>')
                    .appendTo(div);

                for (let todo of todoList.todos) {
                    let li = $('<li/>')
                        .text(todoList.text)
                        .click(function () { invertTodoChecked(todo); })
                        .appendTo(ul);
                    let input = $('<input/>')
                        .prop('type', 'checkbox')
                        .prop('checked', todo.isComplete)
                        .click(function($event) {
                            invertTodoChecked(todo);
                            $event.stopPropagation();
                            $event.preventDefault();
                        })
                        .change(function () { invertTodoChecked(todo); })
                        .appendTo(li);
                    let span = $('<span/>')
                        .text(todo.text)
                        .appendTo(li);
                }
            }

            $("#todoLists").css("display", "block");
            $("#todoListsLoader").css("display", "none");
        });

}

$(document).ready(function () {
    reloadTodoLists();
});
