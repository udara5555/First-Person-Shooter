import { Room, Client } from "colyseus";
import { MyRoomState, FPSPlayer } from "./schema/MyRoomState.js";

export class MyRoom extends Room {
    private get s(): MyRoomState {
        return this.state as MyRoomState;
    }

    onCreate(options: any) {
        this.setState(new MyRoomState());
        console.log("ROOM CREATED:", this.roomId);

        // Handle player joining the game
        this.onMessage("joinGame", (client: Client) => {
            // Prevent duplicate joins
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
    }

    onJoin(client: Client) {
        console.log("CONNECTED TO ROOM:", client.sessionId);
        // Player connected to room but not yet joined the game
        // They will join when they send the "joinGame" message
    }

    onLeave(client: Client) {
        console.log("LEAVE:", client.sessionId);
        this.s.players.delete(client.sessionId);
    }
}