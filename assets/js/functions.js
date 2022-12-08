function finalizeMove(from, to) {
	submitForm(`${from}-${to}`);
}

function finalizePromotionMove(from, to) {
	submitForm(`${from}-${to}p`);
}

function askPromotionFirst(from, to) {
	var yesButton = document.getElementById("do-promote");
	yesButton.setAttribute("onclick", `finalizePromotionMove("${from}", "${to}")`);
	var noButton = document.getElementById("dont-promote");
	noButton.setAttribute("onclick", `finalizeMove("${from}", "${to}")`);
	document.getElementById("ask-promotion-overlay").style.display = "block";
}

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

function selectMoves(forId) {
	resetMoves();
	var old = sessionStorage.getItem("last");
	if (old == forId) {
		sessionStorage.clear();
		return;
	}
	var squares = squareMoveDict[forId];
	if (squares !== undefined) {
		for (let i = 0; i < squares.length; i++) {
			var curId = squares[i];
			var elem = document.getElementById(curId);
			elem.style.setProperty("background-color", "#7fffbf");
			if (shouldAskForPromotion(forId, curId)) {
				elem.setAttribute("onclick", `askPromotionFirst("${forId}","${curId}")`);
			}
			else {
				elem.setAttribute("onclick", `finalizeMove("${forId}","${curId}")`);
			}
		}
		sessionStorage.setItem("last", forId);
	}
}

function shouldAskForPromotion(from, to) {
	var elem = document.getElementById(from);
	var isPromotable = elem.classList.contains("promotable");
	var forcePromo = false;
	var forcePromoteList;
	if (elem.classList.contains("forcePromo1")) {
		forcePromoteList = pawnLancePromo;
	}
	if (elem.classList.contains("forcePromo2")) {
		forcePromoteList = knightPromo;
	}
	if (forcePromoteList !== undefined) {
		for (let i = 0; i < forcePromoteList.length; i++) {
			if (to.includes(forcePromoteList[i])) {
				forcePromo = true;
			}
		}
	}
	return isPromotable && !forcePromo && isInPromotionZone(to);
}

function isInPromotionZone(curId) {
	for (let i = 0; i < promotionZone.length; i++) {
		if (curId.includes(promotionZone[i])) {
			return true;
		}
	}
	return false;
}

function resetMoves() {
	var elems = document.getElementsByClassName("square");
	for (let i = 0; i < elems.length; i++) {
		var elem = elems[i];
		elem.removeAttribute("onclick");
		elem.removeAttribute("style");
	}
	var keys = Object.keys(squareMoveDict);
	for (let i = 0; i < keys.length; i++) {
		var key = keys[i];
		document.getElementById(key).setAttribute("onclick", `selectMoves("${key}")`);
	}
}