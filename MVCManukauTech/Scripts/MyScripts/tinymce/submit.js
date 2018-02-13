/*************************
20 - 08 - 17 *
Reference:
https://www.dyclassroom.com/tinymce/how-to-setup-tinymce-text-editor
https://www.dyclassroom.com/tinymce/how-to-get-data-from-tinymce-text-editor

Task:
click Submit button in Home.page and disaply on data-container text
*************************/
var content = '';
$(document).ready(

    function () {
        
        //display the change text 
        $("#get-data-form").submit(function (e) {
            //get content from current TinyMCE Editor
            content = tinymce.activeEditor.getContent();
            content = content.replace(/>/g, "&xgt;");
            document.getElementById("ContentText").value = content.replace(/</g, "&xlt;");
            //close TinyMCE editor
            $("#texteditor").style.display = "none";
            //return false so page will no looping
            return false;
        });
    }
);


