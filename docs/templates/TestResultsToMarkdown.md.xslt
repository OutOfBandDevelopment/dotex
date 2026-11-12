<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:tt="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
	<xsl:output method="text" indent="no"/>

	<xsl:key name="definition" match="tt:UnitTest" use="@id"/>

	<xsl:template match="/">
		<xsl:variable name="results" select="/tt:TestRun/tt:Results/tt:UnitTestResult"/>
		<xsl:text># Test results&#10;&#10;</xsl:text>
		<xsl:text>**Table 1 — Totals**&#10;&#10;</xsl:text>
		<xsl:text>| Outcome | Tests |&#10;|---------|------:|&#10;</xsl:text>
		<xsl:text>| Total | </xsl:text><xsl:value-of select="count($results)"/><xsl:text> |&#10;</xsl:text>
		<xsl:text>| Passed | </xsl:text><xsl:value-of select="count($results[@outcome='Passed'])"/><xsl:text> |&#10;</xsl:text>
		<xsl:text>| Failed | </xsl:text><xsl:value-of select="count($results[@outcome='Failed'])"/><xsl:text> |&#10;</xsl:text>
		<xsl:text>| Inconclusive | </xsl:text><xsl:value-of select="count($results[@outcome='Inconclusive'])"/><xsl:text> |&#10;</xsl:text>
		<xsl:text>| Skipped | </xsl:text><xsl:value-of select="count($results[@outcome='NotExecuted'])"/><xsl:text> |&#10;&#10;</xsl:text>

		<xsl:text>**Table 2 — Totals per tier**&#10;&#10;</xsl:text>
		<xsl:text>| Tier | Tests | Passed | Failed | Inconclusive | Skipped |&#10;|------|------:|-------:|-------:|-------------:|--------:|&#10;</xsl:text>
		<xsl:call-template name="tier"><xsl:with-param name="name" select="'Unit'"/></xsl:call-template>
		<xsl:call-template name="tier"><xsl:with-param name="name" select="'Simulate'"/></xsl:call-template>
		<xsl:call-template name="tier"><xsl:with-param name="name" select="'Integration'"/></xsl:call-template>
		<xsl:call-template name="tier"><xsl:with-param name="name" select="'Hardware'"/></xsl:call-template>
		<xsl:call-template name="tier"><xsl:with-param name="name" select="'DevLocal'"/></xsl:call-template>
		<xsl:call-template name="tier"><xsl:with-param name="name" select="'LiveIntegration'"/></xsl:call-template>
		<xsl:text>&#10;</xsl:text>

		<xsl:call-template name="list">
			<xsl:with-param name="title" select="'Failed'"/>
			<xsl:with-param name="items" select="$results[@outcome='Failed']"/>
			<xsl:with-param name="withMessage" select="true()"/>
		</xsl:call-template>
		<xsl:call-template name="list">
			<xsl:with-param name="title" select="'Inconclusive'"/>
			<xsl:with-param name="items" select="$results[@outcome='Inconclusive']"/>
			<xsl:with-param name="withMessage" select="true()"/>
		</xsl:call-template>
		<xsl:call-template name="list">
			<xsl:with-param name="title" select="'Skipped'"/>
			<xsl:with-param name="items" select="$results[@outcome='NotExecuted']"/>
			<xsl:with-param name="withMessage" select="false()"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="tier">
		<xsl:param name="name"/>
		<xsl:variable name="in" select="/tt:TestRun/tt:Results/tt:UnitTestResult[key('definition', @testId)/tt:TestCategory/tt:TestCategoryItem/@TestCategory = $name]"/>
		<xsl:if test="$in">
			<xsl:text>| </xsl:text><xsl:value-of select="$name"/>
			<xsl:text> | </xsl:text><xsl:value-of select="count($in)"/>
			<xsl:text> | </xsl:text><xsl:value-of select="count($in[@outcome='Passed'])"/>
			<xsl:text> | </xsl:text><xsl:value-of select="count($in[@outcome='Failed'])"/>
			<xsl:text> | </xsl:text><xsl:value-of select="count($in[@outcome='Inconclusive'])"/>
			<xsl:text> | </xsl:text><xsl:value-of select="count($in[@outcome='NotExecuted'])"/>
			<xsl:text> |&#10;</xsl:text>
		</xsl:if>
	</xsl:template>

	<xsl:template name="list">
		<xsl:param name="title"/>
		<xsl:param name="items"/>
		<xsl:param name="withMessage"/>
		<xsl:if test="$items">
			<xsl:text>## </xsl:text><xsl:value-of select="$title"/><xsl:text>&#10;&#10;</xsl:text>
			<xsl:for-each select="$items">
				<xsl:sort select="key('definition', @testId)/tt:TestMethod/@className"/>
				<xsl:sort select="@testName"/>
				<xsl:variable name="message" select="tt:Output/tt:ErrorInfo/tt:Message"/>
				<xsl:text>- `</xsl:text><xsl:value-of select="key('definition', @testId)/tt:TestMethod/@className"/>
				<xsl:text>.</xsl:text><xsl:value-of select="@testName"/><xsl:text>`</xsl:text>
				<xsl:if test="$withMessage and $message">
					<xsl:text> — </xsl:text>
					<xsl:choose>
						<xsl:when test="contains($message, '&#10;')"><xsl:value-of select="normalize-space(substring-before($message, '&#10;'))"/></xsl:when>
						<xsl:otherwise><xsl:value-of select="normalize-space($message)"/></xsl:otherwise>
					</xsl:choose>
				</xsl:if>
				<xsl:text>&#10;</xsl:text>
			</xsl:for-each>
			<xsl:text>&#10;</xsl:text>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>
