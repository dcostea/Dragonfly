var apiUrl = `/api/v1/default/`;

document.addEventListener('DOMContentLoaded', (event) => {

    document.querySelector("#inspect_models").onclick = function (e) {
        getFileContent("modelinput/file", "#modelinput");
        getFileContent("modeloutput/file", "#modeloutput");
        getSchema("modelinput/schema");
    };

    document.querySelector("#inspect_predictor").onclick = function () {
        getFileContent("predictor/file", "#predictor");
    };

    document.querySelector("#inspect_predictor_withpool").onclick = function () {
        getFileContent("program/file", "#program");
    };

    document.querySelector("#predict").onclick = function () {

        let features = document.querySelectorAll("#features input");
        let sample = "";
        sample += "{ ";

        for (var i = 0; i < features.length;i++) {
            sample += `"${features[i].id}": ${features[i].value}`;
            if (i != features.length - 1) {
                sample += ", ";
            }
        }
        sample += " }";

        postPredict("predict", "#prediction", sample);
    };
});

function postPredict(relativeUrl, target, sample) {

    var y = sample;

    var url = `${apiUrl}${relativeUrl}`;

    fetch(url, {
        method: 'POST',
        mode: 'cors',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        //body: JSON.stringify(sample)
        body: sample
    })
        .then((response) => response.text())
        .then(function (data) {
            if (data !== undefined) {
                let prediction = JSON.parse(data);
                document.querySelector(target).innerHTML = prediction.prediction + " " + prediction.score;
            }
        }).catch(function (error) {
            console.log(error);
            document.querySelector(target).innerHTML = "";
        });
}

function getFileContent(relativeUrl, target) {

    var url = `${apiUrl}${relativeUrl}`;

    fetch(url, {
        method: 'GET',
        mode: 'cors'
    })
        .then((response) => response.text())
        .then(function (data) {
            document.querySelector(target).innerHTML = data;
        })
        .catch(function (error) {
            console.log(error);
            document.querySelector(target).value = "";
        });
}

function getSchema(relativeUrl) {

    var url = `${apiUrl}${relativeUrl}`;

    fetch(url, {
        method: 'GET',
        mode: 'cors'
    })
        .then((response) => response.text())
        .then(function (data) {
            let html = "";
            let features = JSON.parse(data);
            for (let feature of features) {
                html += `<label for="${feature}">${feature}</label><input class="form-control m-2" type="text" id="${feature}" />`;
            }
            document.getElementById("features").innerHTML = html;
        })
        .catch(function (error) {
            console.log(error);
        });
}

//function getModel(relativeUrl, target) {

//    var url = `${apiUrl}`;

//    fetch(url, {
//        method: 'GET',
//        mode: 'cors'
//    })
//        .then((response) => response.text())
//        .then(function (data) {
//            let features = JSON.parse(data);
//            let html = "";
//            for (let feature of features) {
//                html += `<tr>`;
//                html += `<td>${feature.index}</td>`;
//                html += `<td>${feature.name}</td>`;
//                html += `<td>${feature.isHidden}</td>`;
//                html += `<td>${feature.type}</td>`;
//                html += `<td>${feature.annotations}</td>`;
//                html += `</tr>`;
//            }
//            document.querySelector(target).innerHTML = html;
//        })
//        .catch(function (error) {
//            console.log(error);
//            document.querySelector(target).value = "";
//        });
//}


