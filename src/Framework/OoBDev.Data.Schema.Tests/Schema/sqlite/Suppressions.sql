CREATE TABLE "Suppressions" (
    "Address" TEXT NOT NULL CONSTRAINT "PK_Suppressions" PRIMARY KEY,
    "Reason" TEXT NOT NULL,
    "AddedAtTicks" INTEGER NOT NULL
);
