# _Shōgi Website_

---

### _Short Explanation_

- play Shōgi
- as a Website
- written in C#
- localhost only (as of now)

---

### _Longer Explanation_

This project is a game of Shōgi, a variant of chess originating from Japan, that is mostly written in C#.
The server sends HTML documents to the client, and the client sends information back that tells the
server on what square the user clicked. The information sent to the server is then checked, to see if
and what to do.

Click on a piece of the current player to select it.
Click on another piece to change the selected piece.
Click on an empty square to deselect the current piece.
Click on a highlighted square to try and move the selected piece to it.

---

### _Rules_

- player 1 (black or lower player) begins.

---

### _Movements_

> - ↑ : one square
> - ⇑ : multipe squares
> - ↰ : 2 forward and 1 to the side

| Piece | Kanji | Movement | Pictogram |
| --- | --- | --- | --- |
| Pawn | 歩 | front | ↑ |
| promoted Pawn | と | same as Gold General (金) | ↑↓←→⬉⬈ |
| Lance | 香 | multiple in front | ⇑ |
| promoted Lance | 杏/仝 | same as Gold General (金) | ↑↓←→⬉⬈ |
| Knight | 桂 | 2 in front and then to the side | ↰↱ |
| promoted Knight | 圭/今 | same as Gold General (金) | ↑↓←→⬉⬈ |
| Silver General | 銀 | front, front left, front right, back left, back right | ↑⬉⬈⬊⬋ |
| promoted Silver | 全 | same as Gold General (金) | ↑↓←→⬉⬈ |
| Gold General | 金 | front, front left, front right, left, right, back | ↑↓←→⬉⬈ |
| Bishop | 角 | multiple in all diagonal directions | ⇖⇗⇘⇙ |
| promoted Bishop / Horse | 馬 | Bishop (角) and King (王 or 玉) | ↑↓←→⇖⇗⇘⇙ |
| Rook | 飛 | multiple in all straight directions | ⇑⇓⇐⇒ |
| promoted Rook / Dragon | 龍/竜 | Rook (飛) and King (王 or 玉) | ⇑⇓⇐⇒⬉⬈⬊⬋ |
| King | 王/玉 | all directions | ↑↓←→⬉⬈⬊⬋ |
