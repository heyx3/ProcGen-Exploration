var gameCanvas = document.getElementById('gameCanvas');
var renderContext = gameCanvas.getContext('2d');

//Useful math:
function lerp(a, b, t) {
    return a + (t * (b - a));
}
function inverseLerp(a, b, x) {
    return (x - a) / (b - a);
}

//Use a class for vector math.
class Vector2 {
    constructor(x, y) {
        this.x = x;
        this.y = y;
    }

    //Add, Subtract, Multiply, Divide
    add(otherVec) { return new Vector2(this.x + otherVec.x, this.y + otherVec.y); }
    subt(otherVec) { return new Vector2(this.x - otherVec.x, this.y - otherVec.y); }
    mulV(otherVec) { return new Vector2(this.x * otherVec.x, this.y * otherVec.y); }
    mulF(scale) { return new Vector2(this.x * scale, this.y * scale); }
    divV(otherVec) { return new Vector2(this.x / otherVec.x, this.y / otherVec.y); }
    divF(denom) { return new Vector2(this.x / denom, this.y / denom); }

    //Other operators:

    dot(otherVec) {
        return (this.x * otherVec.x) + (this.y * otherVec.y);
    }

    get angleRadians() { return Math.atan2(this.y, this.x); }
    rotated(angle) {
        var newAngle = this.angleRadians + angle;
        return new Vector2(Math.cos(newAngle), Math.sin(newAngle));
    }

    get lengthSqr() { return this.dot(this); }
    get length() { return Math.sqrt(this.lengthSqr); }
    get normalized() {
        var len = this.length;
        return this.divF(len);
    }

    get neg() { return new Vector2(-this.x, -this.y); }
    get perp() { return new Vector2(-this.y, this.x); }

    get asString() {
        return "{" + this.x + ", " + this.y + "}";
    }
}

//Use a class for 0-1 RGB color.
class ColorF {
    constructor(rFloat, gFloat, bFloat) {
        this.r = rFloat;
        this.g = gFloat;
        this.b = bFloat;
    }

    get asString() {
        return 'rgb(' + Math.floor(this.r * 255) + ',' +
                        Math.floor(this.g * 255) + ',' +
                        Math.floor(this.b * 255) + ')';
    }

    tint(otherColor) {
        return new ColorF(this.r * otherColor.r, this.g * otherColor.g, this.b * otherColor.b);
    }
    scale(scaleF) {
        return new ColorF(this.r * scaleF, this.g * scaleF, this.b * scaleF);
    }
}


//Keyboard input is stored as a dictionary (a.k.a. an object).
//If the key code doesn't exist in the dictionary yet, it's considered not pressed.
var keyPressedStatus = {};
function isKeyDown(keyCode) {
    return keyPressedStatus[keyCode] === true;
}
document.addEventListener('keydown', function(event) {
    keyPressedStatus[event.key] = true;
});
document.addEventListener('keyup', function(event) {
    keyPressedStatus[event.key] = false;
});


//Game constants are globals.
const GAME_moveAccel = 500,
      GAME_turnSpeed = 3,
      GAME_playerRadius = 15,
      GAME_playerRadiusSqr = GAME_playerRadius * GAME_playerRadius,
      GAME_playerFriction = 0.95,
      GAME_wallBounceSpeedLoss = 1.3,
      GAME_playerBounceWinSpeed = 0.5,
      GAME_playerHurtMoveAngleMax = 1.0,
      GAME_playerHurtMoveSpeedMin = 400,
      GAME_playerHurtMoveSpeedMax = 700;


//Rendering constants are also globals.
const RENDER_playerLineThickness = 4.0,
      RENDER_playerLineDarkness = 0.2,
      RENDER_playerLineDash = [5, 2],
      RENDER_playerPoleLength = 16,
      RENDER_playerPoleThickness = 4;


//A player is represented as a class.
class Player {

    //'forwardInputGetter' and 'turnInputGetter' should be functions
    //    that return a number between -1 and +1.
    //Human players should use functions that check keyboard input.
    //AI players should use functions that make decisions with AI.
    constructor(pos, forward, tint,
                forwardInputGetter, turnInputGetter) {
        this.pos = pos;
        this.forward = forward;
        this.forwardInputGetter = forwardInputGetter;
        this.turnInputGetter = turnInputGetter;
        this.tint = tint;
        this.velocity = new Vector2(0, 0);
    }

    //Moves and turns the player based on his input functions.
    update(deltaT) {

        //TODO: A button that turns off friction when held.

        //Turn based on input.
        var turnInput = this.turnInputGetter() * GAME_turnSpeed * deltaT;
        this.forward = this.forward.rotated(turnInput).normalized;

        //Accelerate based on input.
        var forwardInput = this.forwardInputGetter();
        var accel = this.forward.mulF(forwardInput * GAME_moveAccel);

        //Update velocity using acceleration and friction.
        this.velocity = this.velocity.mulF(GAME_playerFriction);
        this.velocity = this.velocity.add(accel.mulF(deltaT));

        //Update position using velocity.
        this.pos = this.pos.add(this.velocity.mulF(deltaT));

        //Check for collisions against the sides of the map.
        //If a collision happens, bounce and push off the wall a little bit.
        if (this.pos.x <= GAME_playerRadius) {
            if (this.velocity.x < 0)
                this.velocity.x = -this.velocity.x;
            this.velocity.x *= GAME_wallBounceSpeedLoss;

            this.pos.x = GAME_playerRadius + 1;
        }
        else if (this.pos.x >= gameCanvas.width - GAME_playerRadius) {
            if (this.velocity.x > 0)
                this.velocity.x = -this.velocity.x;
            this.velocity.x *= GAME_wallBounceSpeedLoss;

            this.pos.x = gameCanvas.width - GAME_playerRadius - 1;
        }
        if (this.pos.y <= GAME_playerRadius) {
            if (this.velocity.y < 0)
                this.velocity.y = -this.velocity.y;
            this.velocity.y *= GAME_wallBounceSpeedLoss;

            this.pos.y = GAME_playerRadius + 1;
        }
        else if (this.pos.y >= gameCanvas.height - GAME_playerRadius) {
            if (this.velocity.y > 0)
                this.velocity.y = -this.velocity.y;
            this.velocity.y *= GAME_wallBounceSpeedLoss;

            this.pos.y = gameCanvas.height - GAME_playerRadius - 1;
        }

        //Check for collisions against other players.
        //If a collision happens, figure out who wins, and hurt the other one.
        var thisPlayer = this;
        players.forEach(function(p) {
            if (p !== thisPlayer &&
                p.pos.subt(thisPlayer.pos).lengthSqr < GAME_playerRadiusSqr) {
                    //Give each player a "score" for how well they hit.
                    var thisPlayerScore = thisPlayer.velocity.length *
                                          p.pos.subt(thisPlayer.pos)
                                               .dot(thisPlayer.forward),
                        otherPlayerScore = p.velocity.length *
                                           thisPlayer.pos.subt(p.pos)
                                                         .dot(p.forward);
                    //If it's a tie, choose a winner randomly.
                    if (thisPlayerScore == otherPlayerScore)
                        thisPlayerScore += Math.random() - 0.5;

                    //Process the result.
                    //Note that this will always separate the players
                    //    so they are no longer colliding.
                    if (thisPlayerScore > otherPlayerScore)
                        thisPlayer.hurt(p);
                    else if (thisPlayer < otherPlayerScore)
                        p.hurt(thisPlayer);
            }
        });
    }

    hurt(playerToHurt) {
    
        //TODO: Debug
        
        //Split this player's velocity into the component
        //    towards/away from the other player,
        //    and the component perpendicular to the other player.
        const towardsTarget = playerToHurt.pos.subt(this.pos),
              dist = towardsTarget.length,
              towardsTargetN = towardsTarget.divF(dist);
        var thisVelParallel = towardsTargetN.mulF(towardsTargetN.dot(this.velocity)),
            thisVelPerp = this.velocity - thisVelParallel;
        var toHurtVelParallel = towardsTargetN.mulF(towardsTargetN.dot(playerToHurt.velocity)),
            toHurtVelPerp = playerToHurt.velocity - toHurtVelParallel;

        //Push the target off of this player so they're not touching.
        playerToHurt.pos = this.pos.add(towardsTargetN.mulF((GAME_playerRadius * 2) + 1));

        //Reverse and scale this player's "parallel" velocity to be away from the target.
        thisVelParallel = thisVelParallel.mulF(-GAME_playerBounceWinSpeed);

        //Violently shove the target.
        playerToHurt.velocity = towardsTargetN.mulF(lerp(GAME_playerHurtMoveSpeedMin,
                                                         GAME_playerHurtMoveSpeedMax,
                                                         Math.random()))
                                              .rotated(lerp(-GAME_playerHurtMoveAngleMax,
                                                            GAME_playerHurtMoveAngleMax,
                                                            Math.random()));

        //Apply the changes to the velocity.
        this.velocity = thisVelParallel.add(thisVelPerp);
    }
}


//Generates an input function for a human player.
function makeInputAxis(positiveKeyStr, negativeKeyStr) {
    return function() {
        var value = 0.0;
        if (isKeyDown(positiveKeyStr))
            value += 1;
        if (isKeyDown(negativeKeyStr))
            value -= 1;
        return value;
    }
}


//All players in the game are stored in a global array.
const playerStartBorder = 20;
var players = [
    //Human using WASD.
    new Player(new Vector2(playerStartBorder, playerStartBorder),
               new Vector2(1, 1).normalized,
               new ColorF(0.2, 0.9, 0.375),
               makeInputAxis('w', 's'),
               makeInputAxis('d', 'a')),

    //Human using arrow keys.
    new Player(new Vector2(gameCanvas.clientWidth - playerStartBorder,
                           gameCanvas.clientHeight - playerStartBorder),
               new Vector2(-1, -1).normalized,
               new ColorF(1.0, 0.3, 0.3),
               makeInputAxis('ArrowUp', 'ArrowDown'),
               makeInputAxis('ArrowRight', 'ArrowLeft')),

    //TODO: AI players.
];


//Game loop:
const intervalMilliseconds = 1000/30,
      intervalSeconds = intervalMilliseconds / 1000;
setInterval(updateGame, intervalMilliseconds);
function updateGame() {

    //Update the players.
    players.forEach(function(p) { p.update(intervalSeconds); });

    //Render the game.
    renderContext.clearRect(0, 0, gameCanvas.width, gameCanvas.height);
    players.forEach(function(p) {

        //Render a circle.
        renderContext.fillStyle = p.tint.asString;
        renderContext.strokeStyle = p.tint.scale(RENDER_playerLineDarkness).asString;
        renderContext.lineWidth = RENDER_playerLineThickness;
        renderContext.resetTransform();
        renderContext.translate(p.pos.x, p.pos.y);
        renderContext.rotate(p.forward.angleRadians);
        renderContext.setLineDash(RENDER_playerLineDash);
        renderContext.beginPath();
        renderContext.arc(0 ,0, GAME_playerRadius,
                          0, Math.PI * 2, false);
        renderContext.stroke();
        renderContext.fill();
        renderContext.closePath();

        //Render a forward direction line.
        renderContext.fillStyle = new ColorF(0,0,0).asString;
        renderContext.strokeStyle = new ColorF(0,0,0).asString;
        renderContext.lineWidth = 1;
        renderContext.resetTransform();
        const lineStart = p.pos.add(p.forward.mulF(GAME_playerRadius));
        renderContext.translate(lineStart.x, lineStart.y);
        renderContext.rotate(p.forward.angleRadians);
        renderContext.setLineDash([]);
        renderContext.beginPath();
        renderContext.rect(0, -(RENDER_playerPoleThickness / 2.0),
                           RENDER_playerPoleLength, RENDER_playerPoleThickness);
        renderContext.stroke();
        renderContext.fill();
    });
    
    renderContext.resetTransform();
}