const express = require('express');
const {createServer} = require('http');
const WebSocket = require('ws');
const uuid = require('uuid-random');

const app = express()
const port = 3000

// global variables for the server

app.get("/", function (req, res){
    res.send("Hello there")
})

const httpServer = createServer(app)
const wss = new WebSocket.Server({ server : httpServer });

const septr = "#"
let clients = {}
let games = {}
let enemiesList = []        //global variable

const NetworkConstants = {
    METHOD_JOIN : "on join",
    METHOD_PLAYERCONNECT : "player connect",
    METHOD_PLAY : "play",
    METHOD_NEWPLAYER : "new player join",
    METHOD_SPAWNENEMIES : "spawn enemies",
    METHOD_SPAWNFOREIGNPLAYERS : "spawn foreign player",
    METHOD_PLAYERMOVE : "player move",
    METHOD_PLAYERSHOOT : "shoot",
    METHOD_PLAYERROTATE : "rotate",
    METHOD_HEALTH : "health",
    METHOD_CHAT : "chat",
    METHOD_PLAYERLEFT : "player disconnected",
    METHOD_OLDROOM : "join old game"
}
wss.on('connection', function (ws) {
    let payLoad = {type : "OnJoin"}
    const id = uuid()
    let gameId = ""
    
    payLoad["id"] = id;
    //ws.send(NetworkConstants.METHOD_JOIN+septr+(id));
    console.log(`Sent msg: ${JSON.stringify(payLoad)}\t to user: ${ws}`)
    
    let currentPlayer = {}
    currentPlayer.name = 'unknown'
    
    ws.on('message', function (data){
        let msgData = data.toString().split(septr, [2])
        let msg = ""
        if (msgData[1] !== "")
            msg = JSON.parse(msgData[1])
        let method = msgData[0]
        
        console.log("Received event-> : "+ msgData+"\n")
        
        
        switch (method)
        {
            case NetworkConstants.METHOD_JOIN:
                gameId = uuid() //if new room: id given by server;else: gameId taken by client choice   
                ws.send(NetworkConstants.METHOD_JOIN+septr+(id));
                break
            case NetworkConstants.METHOD_OLDROOM:
                gameId = msg.gameId
                ws.send(NetworkConstants.METHOD_JOIN+septr+(id));
                break
            //a new player has joined a game
            case NetworkConstants.METHOD_SPAWNFOREIGNPLAYERS:
                /*for (let i =0; i< clients.length;i++)
                {
                    let playerConnected = {clients };
                    // sned the info of previously joined player to connected player
                    // refactor this
                    ws.send('other players in the game =>\n'+JSON.stringify(playerConnected))
                    console.log('sent client '+ )
                }*/
                
                // sned the info of previously joined player to connected player
                for (const key of Object.keys(clients)) {
                    if (key !== id) {
                        let playerConnected = clients[key].playerData;
                        console.log(`sending info ${JSON.stringify(playerConnected)} to ${currentPlayer.name}`)
                        ws.send(NetworkConstants.METHOD_SPAWNFOREIGNPLAYERS +
                            septr + JSON.stringify(playerConnected));
                    }                    
                }
                
                break
            //when joined player starts playing the round 
            case NetworkConstants.METHOD_PLAY:
                console.log(currentPlayer.name + ' recv play:'+ (msg))
                //if first player joins game we init the enemies list
                if (Object.keys(clients).length === 0){
                    console.log("player joined first time")
                    noOfEnemies = msg.enemySpawnPoints.length
                             
                    msg.enemySpawnPoints.forEach(function (enemySpawnPoint){
                        let id = uuid()
                        let enemy = {
                            name: id,
                            position: enemySpawnPoint.position,
                            rotation: enemySpawnPoint.rotation,
                            health: 100
                        }
                        enemiesList.push(enemy)
                    })
                    playerSpawnPoints = []      //global variable
                    msg.playerSpawnPoints.forEach(function (_playerSpawnPoints){
                        var playerSpawnPoint = {
                            position : _playerSpawnPoints.position,
                            rotation: _playerSpawnPoints.rotation
                        };
                        playerSpawnPoints.push(playerSpawnPoint)
                    })                    
                }
                let enemyResponse = { enemies : enemiesList }
                //we will always send enemies data when a player joins
                ws.send(NetworkConstants.METHOD_SPAWNENEMIES + septr + JSON.stringify(enemyResponse))
                
                var randomSpawnPoint = playerSpawnPoints[Math.floor(Math.random() * playerSpawnPoints.length)]
                // refactor this data by using currentPlayer and copy it to clients[n].playerGameInfo
                let thisPlayerData = {
                    clientId: id,
                    name : msg.name,
                    position: randomSpawnPoint.position,
                    rotation: randomSpawnPoint.rotation,
                    health: 100
                }
                clients[id] = {
                    socketObject: ws,
                    playerData : thisPlayerData
                };
                
                currentPlayer.name = clients[id].playerData.name
                console.log(currentPlayer.name+' emit-> play:'+ JSON.stringify(clients[id].playerData.clientId));
                //in current game we will send new player joined info to every user
                let newplayer = clients[id]
                for (const key of Object.keys(clients)){
                    clients[key].socketObject.send(NetworkConstants.METHOD_NEWPLAYER + septr 
                        + JSON.stringify(newplayer.playerData))
                    
                    //console.log("Show all clients => "+ clients[key].playerData.name)
                }
                games[gameId] = {clientList : {clients},enemiesList}
                //console.log("All clients of game - "+gameId);
                
                break
            case NetworkConstants.METHOD_PLAYERMOVE:
                console.log("received: move: "+JSON.stringify(msg))
                //update the player position on server
                currentPlayer.position = msg.position
                let moveData = {
                    position: msg.position,
                    clientId: msg.clientId,
                }
                clients[id].playerData.position = msg.position
                //braodcast position for other clients
                /*for (const key of Object.keys(clients)){
                    if (clients[key].playerData.clientId !== msg.clientId)
                        clients[key].socketObject.send(NetworkConstants.METHOD_PLAYERMOVE 
                            + septr+ JSON.stringify(moveData))
                }*/
                BroadcastToOtherClients(id, JSON.stringify(moveData), NetworkConstants.METHOD_PLAYERMOVE
                    + septr)
                /*ws.send(NetworkConstants.METHOD_PLAYERMOVE
                    + septr+ JSON.stringify(moveData))*/
                break
            case NetworkConstants.METHOD_PLAYERROTATE:
                console.log(currentPlayer.name+' received: rotate:'+JSON.stringify(msg))
                let rotateData = {
                    rotation: msg.rotation,
                    clientId: msg.clientId,
                }
                clients[id].playerData.rotation = msg.rotation
                currentPlayer.rotation = msg.rotation
                BroadcastToOtherClients(id, JSON.stringify(rotateData), NetworkConstants.METHOD_PLAYERROTATE
                    + septr)
                break               
            case NetworkConstants.METHOD_PLAYERSHOOT:
                console.log(currentPlayer.name+' received: shoot:')
                let data = {
                    name : currentPlayer.name
                }
                console.log(currentPlayer.name+' command: shoot: '+(id))
                BroadcastToOtherClients(id, (id),
                    NetworkConstants.METHOD_PLAYERSHOOT + septr)
                //ws.send(NetworkConstants.METHOD_PLAYERSHOOT+ septr+ JSON.stringify(id))
                break
            case NetworkConstants.METHOD_HEALTH:
                console.log(currentPlayer.name+' received: health:'+JSON.stringify(msg))
                let playerObj = {}
                let enemyObj = {}
                let entityId
                let response = {}
                let enemyIndex = -1
                let delelteEnemyIndex = -1
                /*if (msg.from === currentPlayer.name) {                    
                    if (!msg.isEnemy) {
                        /!*clients.forEach(_clients => {
                            if (msg.name === _clients.name) {
                                entityId = _clients.clientId
                                _clients.health -= msg.healthChange
                                response = {
                                    name: _clients.name,
                                    health: _clients.health
                                }
                            }
                        })*!/
                        for (const key of Object.keys(clients)) {
                            if (msg.userId === clients[key].playerData.clientId) {
                                entityId = clients[key].clientId
                                clients[key].playerData.health -= msg.healthChange
                                response = {
                                    clientId : clients[key].playerData.clientId, 
                                    name: clients[key].playerData.name,
                                    health: clients[key].playerData.health
                                }
                            }
                        }
                    } else {
                        // to check enemies list obj
                        enemies.forEach(_enemy => {
                            if (msg.userId === _enemy.name) {
                                entityId = _enemy.name
                                _enemy.health -= msg.healthChange
                                response = {
                                    clientId: _enemy.name,
                                    name: _enemy.name,
                                    health: _enemy.health
                                }
                            }
                        })
                        for (e=0;e < enemies.length;e++)
                        {
                            console.log(enemies[e].name);
                        }
                    }                    
                }*/
                
                if (!msg.isEnemy) {
                    for (const key of Object.keys(clients)) {
                        if (msg.userId === clients[key].playerData.clientId) {
                            entityId = clients[key].clientId
                            clients[key].playerData.health -= msg.healthChange
                            response = {
                                clientId : clients[key].playerData.clientId,
                                name: clients[key].playerData.name,
                                health: clients[key].playerData.health
                            }
                        }
                    }
                } else {
                    // to check enemies list obj
                    enemiesList.forEach(_enemy => {
                        enemyIndex++;
                        if (msg.userId === _enemy.name) {
                            entityId = _enemy.name
                            _enemy.health -= msg.healthChange
                            response = {
                                clientId: _enemy.name,
                                name: _enemy.name,
                                health: _enemy.health
                            }
                            if(_enemy.health <= 0 ) 
                                delelteEnemyIndex = enemyIndex
                        }
                    })
                }
                
                //ws.send(NetworkConstants.METHOD_HEALTH+ septr +JSON.stringify(response))
                //broadcast health infp to other players
                BroadcastToOtherClients(id, JSON.stringify(response), NetworkConstants.METHOD_HEALTH+septr)   
                if (delelteEnemyIndex > -1)
                    RemoveEnemyFromGame(delelteEnemyIndex)
                //let _clientToRemove = clients[id]
                //let index = clients.indexOf(_clientToRemove)
                //if (index > -1) clients.splice(index, 1)
                
                break
            case NetworkConstants.METHOD_CHAT:
                //console.log('new chat msg received ->'+msg.name + ' : ' + msg.chatMsg);
                // just broadcast this player name and chat
                for (const key of Object.keys(clients)) {
                    clients[key].socketObject.send(NetworkConstants.METHOD_CHAT + septr + JSON.stringify(msg))
                }
                break
        }
        
    })
    
    ws.on('close', function (){
        console.log("client left."+ currentPlayer.name+" recv: disconnect: ")
        BroadcastToOtherClients(id, (id), NetworkConstants.METHOD_PLAYERLEFT + septr)
        
        console.log(currentPlayer)
        delete clients[id]
        console.log('Active users:')
        // if no player left than we clear the enemy list obj
        if (Object.keys(clients).length === 0)
        {
            //nemiesList = {}
            enemiesList.length = 0
        }
        for (const key of Object.keys(clients)) {
            console.log('\t' + clients[key].playerData.name + '\n')
        }
    })
})

function BroadcastToOtherClients(playerId, messageObj, method)
{
    for (const key of Object.keys(clients)){
        if (clients[key].playerData.clientId !== playerId) {
            clients[key].socketObject.send(method + messageObj)
            console.log(method + " bcst to all clients => " + clients[key].playerData.name)
        }
    }
}

function RemoveEnemyFromGame(enemyIndex)
{
    enemiesList.splice(enemyIndex,1);
    console.log("Remaining enemies ===>")
    enemiesList.forEach(_enemy => {console.log(_enemy.name)})
}

httpServer.listen(port, function (){
    console.log(`Listening on http://localhost:${port}`);
})