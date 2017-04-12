-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [dbo].[GetErrorFixTimeStats]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetErrorFixTimeStats]
as
begin

	set nocount on;

	select
		[ErrorTypeId],
		[Mean],
		[StandardDeviation],
		[PopulationStandardDeviation]
	from [dbo].[ErrorFixTimeStatistics] with(nolock)

end

