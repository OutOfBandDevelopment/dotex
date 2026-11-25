CREATE TABLE "Suppressions" (
    "Address" character varying(320) NOT NULL,
    "Reason" character varying(200) NOT NULL,
    "AddedAtTicks" bigint NOT NULL,
    CONSTRAINT "PK_Suppressions" PRIMARY KEY ("Address")
);
