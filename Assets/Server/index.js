const functions = require('firebase-functions');
const admin = require('firebase-admin');
admin.initializeApp(functions.config().firebase);

var database = admin.database();

exports.matchmaker = functions.database.ref('matchmaking/{playerId}')
    .onCreate((snap, context) => {

        var gameId = generateGameId();

        database.ref('matchmaking').once('value').then(players => {
            var secondPlayer = null;
            players.forEach(player => {
                if (player.val() === "placeholder" && player.key !== context.params.playerId) {
                    secondPlayer = player;
                }
            });

            if (secondPlayer === null) return null;

            database.ref("matchmaking").transaction(function (matchmaking) {

                // If any of the players gets into another game during the transaction, abort the operation
                if (matchmaking === null || matchmaking[context.params.playerId] !== "placeholder" || matchmaking[secondPlayer.key] !== "placeholder") return matchmaking;

                matchmaking[context.params.playerId] = gameId;
                matchmaking[secondPlayer.key] = gameId;
                return matchmaking;

            }).then(result => {

                if (result.snapshot.child(context.params.playerId).val() !== gameId) return;
                 
                var game = {
                    gameInfo: {
                        gameId: gameId,
                        createOn: getFormattedDate(),
                        cardDeck: createShuffleDeck(),
                        puDeck: createPuDeck(context.params.playerId, secondPlayer.key),
                        powerup: createEmptyPu(),
                        bet: 'empty',
                        playersIds: [context.params.playerId, secondPlayer.key]
                    },
                        TurnManager: createTurnMangement(context.params.playerId, secondPlayer.key),
                    turn: context.params.playerId
                }

                database.ref("games/" + gameId).set(game).then(snapshot => {

                    console.log("Game created successfully!")
                    return null;
                }).catch(error => {
                    console.log(error);
                });

                return null;

            }).catch(error => {
                console.log(error);
            });

            return null;
        }).catch(error => {
            console.log(error);
        });
    });

    exports.generateNewDeck = functions.https.onCall((data) => {
  const gameId = data.gameId;
  if (gameId.length === 0) {
         throw new functions.https.HttpsError('invalid-argument', 'GameId empty');
         }
  var cardDeck = {
              cardDeck: createShuffleDeck(),
              }
  database.ref("games/" + gameId+ "/gameInfo/").update(cardDeck).then(snapshot => {
                    console.log("Game created successfully!")
                    return null;
              }).catch(error => {
                    console.log(error);
              });
              });



function generateGameId() {
    var possibleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var gameId = "";
    for (var j = 0; j < 20; j++) gameId += possibleChars.charAt(Math.floor(Math.random() * possibleChars.length));
    return gameId;
}

function createPuDeck(player1Id, player2Id) {
 
    var fPus = ["f1","f2","f3"];
    var wPus = ["w1","w2","w3"];
    var iPus = ["i1","i2","i3"];
    var tPus = ["t1","t2","t3","t4","t5","t6"];

    var fMPus = ["fm1","fm2"];
    var wMPus = ["wm1","wm2"];
    var iMPus = ["im1","im2"];
    var tMPus = ["tm1"];

    var deckOptions ="wtfi";
    var deckPus = [];
    var stringChosenOption;
    var f = 0;
    var w = 0;
    var i = 0;
    var t = 0;

    var playerElement =  player1Id.charAt(0);
    var enemyElement = player2Id.charAt(0);
    var ncs = playerElement;
    if(playerElement!= enemyElement)
    {
        ncs += enemyElement;
    }
    if(!ncs.includes("t")){
        ncs = "fiw";
    }
    else
       {
        deckOptions = shuffleWord(deckOptions);
        for(z = 0; ncs.length<3 ; z++)
             {
             if(!ncs.includes(deckOptions.charAt(z)))
                 {
                     ncs+= deckOptions.charAt(z);
                 }
             }
       }

    stringChosenOption=ncs;

    for(x = 0; deckPus.length < 56; x){
         if(stringChosenOption.includes("f")){
              deckPus[x++] = fPus[f];
              f++;
              if(f == fPus.length){
                    f = 0;
              }
         }
         if(stringChosenOption.includes("w")){
              deckPus[x++] = wPus[w];
              w++;
              if(w == wPus.length){
                    w = 0;
              }
         }
         if(stringChosenOption.includes("i")){
              deckPus[x++] = iPus[i];
              i++;
              if(i == iPus.length){
                    i = 0;
              }
         }
         if(stringChosenOption.includes("t")){
              deckPus[x++] = tPus[t];
              t++;
              if(t == tPus.length){
                    t = 0;
              }
         }
    }

      if(stringChosenOption.includes("f")){
              deckPus = deckPus.concat(fMPus);
         }
      if(stringChosenOption.includes("w")){
              deckPus = deckPus.concat(wMPus);
         }
      if(stringChosenOption.includes("i")){
               deckPus = deckPus.concat(iMPus);
         }
      if(stringChosenOption.includes("t")){
               deckPus = deckPus.concat(tMPus);
         }
    
    return shuffleArray(deckPus);
}

function createShuffleDeck() {
    var deck = ["Ac","2c","3c","4c","5c","6c","7c","8c","9c","Tc","Jc","Qc","Kc",
                "Ad","2d","3d","4d","5d","6d","7d","8d","9d","Td","Jd","Qd","Kd",
                "As","2s","3s","4s","5s","6s","7s","8s","9s","Ts","Js","Qs","Ks",
                "Ah","2h","3h","4h","5h","6h","7h","8h","9h","Th","Jh","Qh","Kh"];
    return shuffleArray(deck);
}

function shuffleArray(array) {
    for (let i = array.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [array[i], array[j]] = [array[j], array[i]];
    }
    return array;
}

function getFormattedDate(){
    var d = new Date();
    d = ('0' + d.getHours()).slice(-2) + ":" + ('0' + d.getMinutes()).slice(-2) + ":" + ('0' + d.getSeconds()).slice(-2) + " " + ('0' + d.getDate()).slice(-2) + "-" + ('0' + (d.getMonth() + 1)).slice(-2) + "-" + d.getFullYear();
    return d;
}

function createEmptyPu(){
    var powerup = {
        playerId: 'empty',
        powerupName: 'empty',
        cardPlace1: 'empty',
        cardPlace2: 'empty',
        slot: -1,
        timeStamp: 0
        };
    return powerup;
    }

    function shuffleWord (word){
        var shuffledWord = '';
        word = word.split('');
         while (word.length > 0) {
               shuffledWord +=  word.splice(word.length * Math.random() << 0, 1);
      }
    return shuffledWord;
    }

function createTurnMangement(player1,player2){
    var TurnManager = {
        CurrentPlayer: player1,
        TurnCounter: 6,
        FirstPlayer: player1,
        SecondPlayer: player2,
        RoundCounter: 1,
        Player1_Ready: -1,
        Player2_Ready: -1
        };
    return TurnManager;
    }


