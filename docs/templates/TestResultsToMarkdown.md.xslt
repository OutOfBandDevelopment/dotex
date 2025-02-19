<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE xsl:stylesheet [
	<!ENTITY cr "<xsl:text>
</xsl:text>">
]>
<xsl:stylesheet
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:tt="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	>
	<xsl:output method="text" indent="no"/>

	<xsl:template match="/">
		<xsl:text># Test Results</xsl:text>&cr;&cr;

		<!--<TestRun id="e0d5c8c7-76dc-4bf4-8c92-8a4fbf6a0eba" name="MWhited@LW-18560 2023-07-12 11:06:34" runUser="EGI\MWhited" 
xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Times 
  creation="2023-07-12T11:06:34.8838123-04:00" 
  queuing="2023-07-12T11:06:34.8838123-04:00" 
  start="2023-07-12T11:06:27.0160661-04:00" 
  finish="2023-07-12T11:06:43.4406360-04:00" 
  />
  <TestSettings name="default" id="3766080a-bc96-4439-853d-0af501a6f44b">
    <Deployment runDeploymentRoot="MWhited_LW-18560_2023-07-12_11_06_34" />
  </TestSettings>-->

		<xsl:apply-templates select="/tt:TestRun/tt:Results/tt:UnitTestResult">
			<!--<xsl:sort select="/tt:TestRun/tt:TestDefinitions/tt:UnitTest[@id=@testId]/tt:TestMethod/@className" />-->
			<xsl:sort select="@testName" />
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="tt:UnitTestResult">

		<!--
		<TestRun><Results>
	<UnitTestResult 
		executionId="85402472-3cbc-4a1e-9ba2-390fef860389" 
		testId="4ec775bf-44bd-e2cb-9974-71998d0ab555" 
		testName="GetHash (hello world,XrY7u+Ae7tCTyyK7j1rNww==)" 
		computerName="LW-18560" 
		duration="00:00:00.0243718" 
		startTime="2023-07-12T11:06:42.7077536-04:00" 
		endTime="2023-07-12T11:06:42.7384692-04:00" 
		testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" 
		outcome="Passed" 
		testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" 
		relativeResultsDirectory="85402472-3cbc-4a1e-9ba2-390fef860389"
		>
		-->

		<xsl:text>## Test Name: </xsl:text><xsl:value-of select="@testName"/>&cr;&cr;

		<xsl:variable name="test-id" select="@testId" />
		<xsl:variable name="execution-id" select="@executionId" />

		<xsl:variable name="unit-test" select="/tt:TestRun/tt:TestDefinitions/tt:UnitTest[@id=$test-id]" />

		<xsl:text>* Name: </xsl:text><xsl:value-of select="$unit-test/@name"/>&cr;
		<!--<xsl:text>* Storage: </xsl:text><xsl:value-of select="$unit-test/@storage"/>&cr;-->
		<xsl:text>* Test Class: </xsl:text><xsl:value-of select="$unit-test/tt:TestMethod/@className"/>&cr;
		<xsl:text>  * Method: </xsl:text><xsl:value-of select="$unit-test/tt:TestMethod/@name"/>&cr;
		<!--<xsl:text>  * Code Base: </xsl:text><xsl:value-of select="$unit-test/tt:TestMethod/@codeBase"/>&cr;-->
		<xsl:if test="$unit-test/tt:TestCategory/tt:TestCategoryItem/@TestCategory">
			<xsl:text>* Categories</xsl:text>&cr;
			<xsl:for-each select="$unit-test/tt:TestCategory/tt:TestCategoryItem/@TestCategory">
				<xsl:text>  * </xsl:text><xsl:value-of select="."/>&cr;
			</xsl:for-each>
		</xsl:if>
		
		<xsl:text>* Details: </xsl:text>&cr;	
		<!--<xsl:text>  * Execution ID: </xsl:text><xsl:value-of select="@executionId"/>&cr;-->	
		<!--<xsl:text>  * Test ID: </xsl:text><xsl:value-of select="@testId"/>&cr;-->	
		<xsl:text>  * Duration: </xsl:text><xsl:value-of select="@duration"/>&cr;	
		<xsl:text>  * Outcome: </xsl:text><xsl:value-of select="@outcome"/>&cr;	
		&cr;

		<xsl:if test="tt:Output/tt:StdOut">
			&cr;
			<xsl:text>### Standard Out</xsl:text>&cr;&cr;
			<xsl:value-of select="tt:Output/tt:StdOut" disable-output-escaping="yes"/>
			&cr;
		</xsl:if>

		<xsl:if test="tt:Output/tt:StdErr">
			&cr;
			<xsl:text>### Standard Error</xsl:text>&cr;&cr;
			<xsl:value-of select="tt:Output/tt:StdErr" disable-output-escaping="yes"/>
			&cr;
		</xsl:if>
		&cr;
	</xsl:template>


	<xsl:template match="@* | node()" mode="duplicate">
		<xsl:copy>
			<xsl:apply-templates select="@* | node()" mode="duplicate"/>
		</xsl:copy>
	</xsl:template>

</xsl:stylesheet>