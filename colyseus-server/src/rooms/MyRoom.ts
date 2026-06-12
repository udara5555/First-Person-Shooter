import { Room, Client } from "colyseus";
import { MyRoomState, FPSPlayer } from "./schema/MyRoomState.js";

export class MyRoom extends Room {
    private get s(): MyRoomState {
        return this.state as MyRoomState;
    }

    // Store player skins when they join room (before game starts)
    private playerSkins: Map<string, string> = new Map();
    private gameTimerInterval: NodeJS.Timeout | null = null;
    private gameStartTime: number = 0;
    private gameDuration: number = 300; // 5 minutes in seconds

    onCreate(options: any) {
        this.setState(new MyRoomState());
        console.log("ROOM CREATED:", this.roomId);

        // Start timer immediately when room is created
        this.gameStartTime = Date.now();
        this.s.gameStartTime = this.gameStartTime;
        this.StartGameTimer();
        console.log("GAME TIMER STARTED AT:", this.gameStartTime);

        this.onMessage("startGame", (client: Client, data: any) => {
            if (!this.s.isGameActive) {
                this.s.isGameActive = true;
                console.log("GAME STARTED (gameplay begins):", client.sessionId);
            }
        });

        this.onMessage("joinGame", (client: Client, data: any) => {
            if (this.s.players.has(client.sessionId)) {
                console.log("PLAYER ALREADY JOINED:", client.sessionId);
                return;
            }

            console.log("PLAYER JOINED GAME:", client.sessionId);
            const player = new FPSPlayer();
            player.x = 0;
            player.y = 0;
            player.z = 0;
            player.skin = this.playerSkins.get(client.sessionId) || "Skin1";
            player.currentWeaponId = data.weaponId || "ak47";
            this.s.players.set(client.sessionId, player);
            console.log("PLAYER SKIN SET TO:", player.skin);
        });

        this.onMessage("move", (client: Client, data: any) => {
            const player = this.s.players.get(client.sessionId);
            if (!player) return;
            player.x = data.x ?? player.x;
            player.y = data.y ?? player.y;
            player.z = data.z ?? player.z;
            player.rotY = data.rotY ?? player.rotY;
            player.isWalking = data.isWalking ?? player.isWalking;
            player.isSprinting = data.isSprinting ?? player.isSprinting;
            player.isReloading = data.isReloading ?? player.isReloading;
            //player.currentWeaponId = data.currentWeaponId ?? player.currentWeaponId;
        });

        this.onMessage("changeSkin", (client: Client, data: any) => {
            const player = this.s.players.get(client.sessionId);
            if (!player) return;
            player.skin = data.skin || "Skin1";
            console.log("PLAYER SKIN CHANGED:", client.sessionId, "to", player.skin);
        });

        this.onMessage("switchWeapon", (client: Client, data: any) => {
            const player = this.s.players.get(client.sessionId);
            if (!player) return;
            player.currentWeaponId = data.weaponId || "ak47";
            console.log("PLAYER SWITCHED WEAPON:", client.sessionId, "to", player.currentWeaponId);

            this.broadcast("weaponSwitched", {
                playerId: client.sessionId,
                weaponId: player.currentWeaponId
            })
        });

        this.onMessage("sprint", (client: Client, data: any) => {
            const player = this.s.players.get(client.sessionId);
            if (!player) return;
            player.isSprinting = data.isSprinting || false;
            console.log("PLAYER SPRINT STATE:", client.sessionId, "isSprinting:", player.isSprinting);
        });

        this.onMessage("reload", (client: Client, data: any) => {
            const player = this.s.players.get(client.sessionId);
            if (!player) return;
            player.isReloading = data.isReloading || false;
            console.log("PLAYER RELOAD STATE:", client.sessionId, "isReloading:", player.isReloading);
        });

        this.onMessage("shoot", (client: Client, data: any) => {
            const player = this.s.players.get(client.sessionId);
            if (!player) return;

            player.isShooting = true;

            // Broadcast shoot event to all OTHER clients for muzzle flash + sound
            this.broadcast("playerShoot", {
                playerId: client.sessionId,
                weaponId: player.currentWeaponId,
                // Shoot origin and direction for remote VFX positioning
                originX: data.originX ?? player.x,
                originY: data.originY ?? player.y,
                originZ: data.originZ ?? player.z,
                dirX: data.dirX ?? 0,
                dirY: data.dirY ?? 0,
                dirZ: data.dirZ ?? 1,
            }, { except: client });

            // Reset isShooting flag after a short delay
            setTimeout(() => {
                player.isShooting = false;
            }, 50);
        });

        this.onMessage("damage", (client: Client, data: any) => {
            const targetPlayer = this.s.players.get(data.targetPlayerId);
            if (!targetPlayer) return;

            const damageAmount = Math.max(0, data.damage);
            targetPlayer.health = Math.max(0, targetPlayer.health - damageAmount);

            this.broadcast("playerDamaged", {
                playerId: data.targetPlayerId,
                health: targetPlayer.health,
                damage: damageAmount
            });

            if (targetPlayer.health <= 0) {
                console.log("PLAYER DIED:", data.targetPlayerId);

                this.broadcast("playerDied", {
                    playerId: data.targetPlayerId
                });

                setTimeout(() => {
                    this.s.players.delete(data.targetPlayerId);
                    this.playerSkins.delete(data.targetPlayerId);
                    console.log("PLAYER REMOVED FROM ROOM:", data.targetPlayerId);
                }, 500);
            }
        });

        this.onMessage("playerKilled", (client: Client, data: any) => {
            const playerId = data.playerId;
            const player = this.s.players.get(playerId);

            if (!player) return;

            console.log("PLAYER KILLED:", playerId);
            player.health = 0;

            this.broadcast("playerDied", {
                playerId: playerId
            });

            setTimeout(() => {
                this.s.players.delete(playerId);
                this.playerSkins.delete(playerId);
                console.log("PLAYER REMOVED FROM ROOM:", playerId);
            }, 500);
        });
    }

    onJoin(client: Client, options: any) {
        console.log("PLAYER JOINED ROOM:", client.sessionId);

        // Store the skin from join options
        if (options.skin) {
            this.playerSkins.set(client.sessionId, options.skin);
        }

        // Add player to game state automatically
        const player = new FPSPlayer();
        player.x = 0;
        player.y = 0;
        player.z = 0;
        player.skin = this.playerSkins.get(client.sessionId) || "default";
        player.currentWeaponId = options.weaponId || "ak47";
        this.s.players.set(client.sessionId, player);
        console.log("PLAYER ADDED TO STATE:", client.sessionId, "with skin:", player.skin, "and weapon:", player.currentWeaponId);

        // Send current game state to newly joined player
        client.send("gameState", {
            gameStartTime: this.s.gameStartTime,
            isGameActive: this.s.isGameActive,
            gameDuration: this.gameDuration
        });
    }

    onLeave(client: Client, code?: number) {
        console.log("PLAYER LEFT ROOM:", client.sessionId);
        this.s.players.delete(client.sessionId);
        this.playerSkins.delete(client.sessionId);
    }

    onDispose() {
        if (this.gameTimerInterval) {
            clearInterval(this.gameTimerInterval);
        }
    }

    private StartGameTimer() {
        this.gameTimerInterval = setInterval(() => {
            const elapsedSeconds = (Date.now() - this.gameStartTime) / 1000;
            const timeRemaining = Math.max(0, this.gameDuration - elapsedSeconds);

            this.s.timeRemaining = timeRemaining;

            if (timeRemaining <= 0) {
                this.EndGame();
            }
        }, 100); // Update every 100ms for smooth updates
    }

    private EndGame() {
        if (this.gameTimerInterval) {
            clearInterval(this.gameTimerInterval);
            this.gameTimerInterval = null;
        }

        this.s.isGameActive = false;
        this.broadcast("gameEnded", { reason: "timeUp" });
        console.log("GAME TIME OVER!");
    }
}