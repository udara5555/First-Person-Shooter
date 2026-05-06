import { Room, Client } from "colyseus";
import { MyRoomState, FPSPlayer } from "./schema/MyRoomState.js";

export class MyRoom extends Room {
    private get s(): MyRoomState {
        return this.state as MyRoomState;
    }

    onCreate(options: any) {
        this.setState(new MyRoomState());
        console.log("ROOM CREATED:", this.roomId);

        this.onMessage("joinGame", (client: Client) => {
            if (this.s.players.has(client.sessionId)) {
                console.log("PLAYER ALREADY JOINED:", client.sessionId);
                return;
            }
            
            console.log("PLAYER JOINED GAME:", client.sessionId);
            const player = new FPSPlayer();
            player.x = 0;
            player.y = 0;
            player.z = 0;
            this.s.players.set(client.sessionId, player);
        });

        this.onMessage("move", (client: Client, data: any) => {
            const player = this.s.players.get(client.sessionId);
            if (!player) return;
            player.x = data.x ?? player.x;
            player.y = data.y ?? player.y;
            player.z = data.z ?? player.z;
            player.rotY = data.rotY ?? player.rotY;
            player.isWalking = data.isWalking ?? player.isWalking;
        });

        this.onMessage("damage", (client: Client, data: any) => {
            const targetPlayer = this.s.players.get(data.targetPlayerId);
            if (!targetPlayer) return;
            
            const damageAmount = Math.max(0, data.damage);
            targetPlayer.health = Math.max(0, targetPlayer.health - damageAmount);
            
            // Broadcast damage event
            this.broadcast("playerDamaged", {
                playerId: data.targetPlayerId,
                health: targetPlayer.health,
                damage: damageAmount
            });

            // Check if player died
            if (targetPlayer.health <= 0) {
                console.log("PLAYER DIED:", data.targetPlayerId);
                
                // Broadcast player death to all clients
                this.broadcast("playerDied", {
                    playerId: data.targetPlayerId
                });

                // Remove player from room after a short delay
                setTimeout(() => {
                    this.s.players.delete(data.targetPlayerId);
                    console.log("PLAYER REMOVED FROM ROOM:", data.targetPlayerId);
                }, 500);
            }
        });
    }

    onJoin(client: Client) {
        console.log("CONNECTED TO ROOM:", client.sessionId);
    }

    onLeave(client: Client) {
        console.log("LEAVE:", client.sessionId);
        this.s.players.delete(client.sessionId);
    }
}