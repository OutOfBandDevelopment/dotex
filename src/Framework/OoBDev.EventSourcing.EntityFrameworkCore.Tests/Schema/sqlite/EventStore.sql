CREATE TABLE "Entities" (
    "Type" TEXT NOT NULL,
    "Key" TEXT NOT NULL,
    "Version" INTEGER NOT NULL,
    "Value" TEXT NOT NULL,
    CONSTRAINT "PK_Entities" PRIMARY KEY ("Type", "Key"),
    CONSTRAINT "CK_Entities_Value_Json" CHECK (json_valid("Value"))
);

CREATE TABLE "Events" (
    "Position" INTEGER NOT NULL CONSTRAINT "PK_Events" PRIMARY KEY AUTOINCREMENT,
    "StreamId" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "Type" TEXT NOT NULL,
    "Timestamp" TEXT NOT NULL,
    "Payload" TEXT NOT NULL,
    "Metadata" TEXT NULL,
    CONSTRAINT "CK_Events_Payload_Json" CHECK (json_valid("Payload"))
);

CREATE UNIQUE INDEX "IX_Events_StreamId_Sequence" ON "Events" ("StreamId", "Sequence");
