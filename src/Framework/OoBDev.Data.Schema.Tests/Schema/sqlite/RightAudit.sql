CREATE TABLE "RightAudit" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_RightAudit" PRIMARY KEY AUTOINCREMENT,
    "AtTicks" INTEGER NOT NULL,
    "Subject" TEXT NOT NULL,
    "Rights" TEXT NOT NULL,
    "Result" TEXT NOT NULL
);

CREATE INDEX "IX_RightAudit_AtTicks" ON "RightAudit" ("AtTicks");

CREATE INDEX "IX_RightAudit_Subject_AtTicks" ON "RightAudit" ("Subject", "AtTicks");
