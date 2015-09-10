function crossclick() {
    var div = this.parentNode;
    div.remove();
}

$('#datetimepicker').datetimepicker({
    dayOfWeekStart: 1,
    lang: 'en',
    disabledDates: ['1986/01/08', '1986/01/09', '1986/01/10'],
    startDate: '1986/01/05'
});

$('#datetimepicker').datetimepicker({ value: '2015/04/15 05:03', step: 10 });

$('#datetimepicker3').datetimepicker({
    format: 'd.m.Y H:i',
    inline: true,
    lang: 'pt',
    theme: 'dark',
    minDate: '-1970/01/01',
   
});


function addImage(res, painel) {
    var select = document.createElement("img");
    var d = new Date();
    $(select).attr("src", res + "?" + d.getTime());
    select.setAttribute("height", "100");
    select.setAttribute("width", "100");
    select.className = "img";

    var div = document.createElement("div");
    div.className = "wrapper";

    div.appendChild(select);
    addCrossAndReplace(div);
    painel.appendChild(div);
}
function selectPictogram(selectPictogram,selectedPainel) {
    if (selectPictogram.length != 0) {
        var select = selectPictogram.cloneNode(true);
        select.className = "img";

        var div = document.createElement("div");
        div.className = "wrapper";

        div.appendChild(select);
        addCrossAndReplace(div);
        selectedPainel.appendChild(div);
    }
}

function addCrossAndReplace(divParent) {

    var cross = document.createElement("img");
    cross.className = "cross-img";
    cross.setAttribute("src", "http://www.clker.com/cliparts/0/7/e/a/12074327311562940906milker_X_icon.svg.med.png");
    cross.onclick = crossclick;

    var replace = document.createElement("img");
    replace.className = "replace";
    replace.setAttribute("src", "https://cdn4.iconfinder.com/data/icons/basic-office-icons/512/change.png");
    replace.onclick = function () {
        var div = this.parentNode;
        var img = $(div).children(".img")[0];

        var newImg = $(".borderline")[0];
        var select = newImg.cloneNode(true);
        select.className = "img";

        $(img).replaceWith(select);
    }


    divParent.appendChild(cross);
    divParent.appendChild(replace);
}

function loadPictogram(name , painel,selectedPainel) {
    var xmlhttp = new XMLHttpRequest();

    var n = name[0].value;
    var array;


    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4) {

            if (xmlhttp.status == 200) {
                var res = JSON.parse(xmlhttp.responseText);

                while (painel.firstChild) {
                    painel.removeChild(painel.firstChild);
                }

                if (res.length != 0) {
                    for (key in res) {
                        var pict = res[key];
                        var img = document.createElement("img");
                        img.setAttribute("src", pict["url"]);
                        img.setAttribute("alt", pict["name"]);
                        img.setAttribute("height", "100");
                        img.setAttribute("width", "100");
                        img.className = "img-rounded";
                        img.addEventListener('dblclick', function (e) {
                            selectPictogram(this, selectedPainel);
                        });
                        img.addEventListener('click', function (e) {
                            array = painel.children;

                            for (i = 0; i < array.length; i++) {
                                array[i].className = "img-rounded";
                            }

                            this.className = 'borderline img-rounded';
                        });

                        painel.appendChild(img);
                    }


                }
            }

            if (xmlhttp.status == 500) {
                if (xhr.responseURL != null) {
                    var newDoc = document.open("text/html", "replace");
                    newDoc.write(xhr.response);
                    newDoc.close();
                }
            }
        }
    }
    var act = "/Home/GetPictogram?pictogram=" + n;

    xmlhttp.open("GET", act);
    xmlhttp.send();


}