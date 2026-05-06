import { Schema, MapSchema, type } from "@colyseus/schema";

export class FPSPlayer extends Schema {
    @type("float32") x: number = 0;
    @type("float32") y: number = 0;
    @type("float32") z: number = 0;
    @type("float32") rotY: number = 0;
    @type("boolean") isWalking = false;
    @type("float32") health: number = 100;
    @type("float32") maxHealth: number = 100;
}

export class MyRoomState extends Schema {
    @type({ map: FPSPlayer }) players = new MapSchema<FPSPlayer>();
}