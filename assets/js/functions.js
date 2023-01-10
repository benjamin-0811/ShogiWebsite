function restart() {
	submitForm("restart");
}

function surrender() {
	submitForm("surrender");
}

function submitForm(value) {
	var form1 = document.getElementById("moveForm");
	var input1 = document.getElementById("move");
	input1.setAttribute("value", value);
	form1.submit();
}