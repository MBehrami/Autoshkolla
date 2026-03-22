-- Stored procedure to deactivate expired CandidateAccounts based on ValidTo date
-- Schedule this to run daily via SQL Agent Job or external scheduler
-- Created: March 18, 2026

IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_DeactivateExpiredCandidateAccounts')
    DROP PROCEDURE sp_DeactivateExpiredCandidateAccounts;

GO

CREATE PROCEDURE sp_DeactivateExpiredCandidateAccounts
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE CandidateAccounts
    SET IsActive = 0
    WHERE IsActive = 1 
      AND ValidTo < CAST(GETDATE() AS DATE);

    -- Return number of rows affected
    SELECT @@ROWCOUNT AS DeactivatedAccountCount;
END;

GO

-- USAGE EXAMPLE (manual execution for testing):
-- EXEC sp_DeactivateExpiredCandidateAccounts;
