CREATE TABLE "ComplexEventLastRun" (
    "Key" TEXT NOT NULL CONSTRAINT "PK_ComplexEventLastRun" PRIMARY KEY,
    "LastRunTicks" INTEGER NOT NULL
);

CREATE TABLE "ComplexEventSchedule" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ComplexEventSchedule" PRIMARY KEY,
    "Key" TEXT NOT NULL,
    "DueAtTicks" INTEGER NOT NULL,
    "EventType" TEXT NOT NULL,
    "Payload" BLOB NOT NULL,
    "Attempts" INTEGER NOT NULL,
    "Failed" INTEGER NOT NULL,
    "LeasedUntilTicks" INTEGER NOT NULL
);

CREATE INDEX "IX_ComplexEventSchedule_Failed_DueAtTicks" ON "ComplexEventSchedule" ("Failed", "DueAtTicks");

CREATE UNIQUE INDEX "IX_ComplexEventSchedule_Key" ON "ComplexEventSchedule" ("Key");
