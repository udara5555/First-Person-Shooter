import { Room, Client } from "colyseus";
import { MyRoomState, FPSPlayer } from "./schema/MyRoomState.js";

export class MyRoom extends Room {
    private get s(): MyRoomState {
        return this.state as MyRoomState;
    }

    onCreate(options: any) {
        this.setState(new MyRoomState());
        console.log("ROOM CREATED:", this.roomId);

        this.onMessage("move", (client: Client, data: any) => {
            const player = this.s.players.get(client.sessionId);
            if (!player) return;
            player.x = data.x ?? player.x;
            player.y = data.y ?? player.y;
            player.z = data.z ?? player.z;
            player.rotY = data.rotY ?? player.rotY;
        });
    }

    onJoin(client: Client) {
        console.log("JOIN:", client.sessionId);
        const player = new FPSPlayer();
        player.x = 0;
        player.y = 0;
        player.z = 0;
        this.s.players.set(client.sessionId, player);
    }

    onLeave(client: Client) {
        console.log("LEAVE:", client.sessionId);
        this.s.players.delete(client.sessionId);
    }
}