CREATE TABLE "ComplexEventLastRun" (
    "Key" character varying(200) NOT NULL,
    "LastRunTicks" bigint NOT NULL,
    CONSTRAINT "PK_ComplexEventLastRun" PRIMARY KEY ("Key")
);

CREATE TABLE "ComplexEventSchedule" (
    "Id" uuid NOT NULL,
    "Key" character varying(200) NOT NULL,
    "DueAtTicks" bigint NOT NULL,
    "EventType" character varying(200) NOT NULL,
    "Payload" bytea NOT NULL,
    "Attempts" integer NOT NULL,
    "Failed" boolean NOT NULL,
    "LeasedUntilTicks" bigint NOT NULL,
    CONSTRAINT "PK_ComplexEventSchedule" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_ComplexEventSchedule_Failed_DueAtTicks" ON "ComplexEventSchedule" ("Failed", "DueAtTicks");

CREATE UNIQUE INDEX "IX_ComplexEventSchedule_Key" ON "ComplexEventSchedule" ("Key");
