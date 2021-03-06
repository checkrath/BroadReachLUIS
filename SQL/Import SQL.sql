/****** Script for SelectTopNRows command from SSMS  ******/

--insert into [dbo].[Districts] 
--select distinct cast([DistrictID] as numeric(19,0)) as DistrictID, [District]
----into [dbo].[Districts]
--from [dbo].[WeeklyReportDT]
--where DistrictID is not null

--insert into [dbo].[Indicators]
--select distinct [Indicator] 
--from [dbo].[WeeklyReportDT]

--insert into [dbo].[Facilities]
select distinct cast([facilityID] as numeric(19,0)) as facilityID, [Facility], cast([DistrictID]as numeric(19,0)) as DistrictID
from [dbo].[WeeklyReportDT]
where FacilityID is not null
--group by ([facilityID])



select sub.facilityID, count(sub.facilityID) facCount from (
select distinct cast([facilityID] as numeric(19,0)) as facilityID,[Facility], cast([DistrictID]as numeric(19,0)) as DistrictID
from [dbo].[WeeklyReportDT]
where FacilityID is not null and Facility not like 'St Patricks Hospital'
	and facility not like 'Mvutshini Clinic (Hibiscus Coast)'
--order by FacilityID
) as sub
group by ( sub.facilityID)
having count(sub.facilityID)>1

insert into [dbo].[Facilities]
select distinct cast([facilityID] as numeric(19,0)) as facilityID,[Facility], cast([DistrictID]as numeric(19,0)) as DistrictID
from [dbo].[WeeklyReportDT]
where FacilityID is not null and Facility not like 'St Patricks Hospital'
	and facility not like 'Mvutshini Clinic (Hibiscus Coast)'
	and facility not like 'St Patricks Gateway Clinic'
order by FacilityID


--select [District],
--	sum(cast([PeriodValue] as numeric(19,2) )) as PeriodValueSum,
--	sum(cast([AnnualTarget] as numeric(19,2) )) as AnnualTargetSum,
--	sum(cast([YTDTarget] as numeric(19,2) )) as YTDTargetSum
-- -- sum(cast([PeriodValue] as numeric(19,0)) 
--from [dbo].[WeeklyReportDT]
--where [Type] like '%Quarterly%' ---'%Quaterley' 
--	AND [PeriodName] like 'July 17 - September 17'
--	--AND [Indicator] like 'HTS_TST'
--	AND [District] is not null
----group by ([District])

--insert into [dbo].[CurrentDistrictMetrics]
--select cast([DistrictID] as numeric(19,0)) as DistrictID,[dbo].[Indicators].[IndicatorID], 
--	[YTDValue],COALESCE( [AnnualTarget],[YTDTarget]) as  AnnualTarget, [YTDTarget]
--from [dbo].[WeeklyReportDT]
--inner join [dbo].[Indicators] on [Indicator] like [IndicatorName]
--where [Type] like '%Quarterly%' ---'%Quaterley' 
--	AND [PeriodName] like 'July 17 - September 17'
--	AND [GeoLevel] like 'District'
--	AND [District] is not null

--insert into [dbo].[CurrentFacilityMetrics]
--select cast([facilityID] as numeric(19,0)) as facilityID,[dbo].[Indicators].[IndicatorID], 
--	Coalesce([YTDValue],0) as YTDValue,COALESCE( [AnnualTarget],[YTDTarget]) as  AnnualTarget, [YTDTarget]
--from [dbo].[WeeklyReportDT]
--inner join [dbo].[Indicators] on [Indicator] like [IndicatorName]
--where [Type] like '%Quarterly%' ---'%Quaterley' 
--	AND [PeriodName] like 'July 17 - September 17'
--	AND [GeoLevel] like 'Facility'








