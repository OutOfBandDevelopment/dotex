<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE xsl:stylesheet [
	<!ENTITY cr "
">
	<!ENTITY tab "    ">
	<!ENTITY sp " ">
]>
<xsl:stylesheet version="1.0"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:bom="http://cyclonedx.org/schema/bom/1.6"
				>
	<xsl:output method="text" omit-xml-declaration="yes" indent="no" />

	<xsl:template match="/">
		<xsl:text># Software Bill of Materials&cr;&cr;</xsl:text>
		<xsl:apply-templates select="bom:bom/bom:metadata"/>
		<xsl:apply-templates select="bom:bom/bom:components"/>
	</xsl:template>

	<xsl:template match="bom:metadata">
		<xsl:text>## Summary &cr;&cr;</xsl:text>
		<xsl:text>Application: </xsl:text>
		<xsl:value-of select="bom:component/bom:name"/>
		<xsl:text> (</xsl:text>
		<xsl:value-of select="bom:component/bom:version"/>
		<xsl:text>) \&cr;</xsl:text>
		<xsl:text>Time: </xsl:text>
		<xsl:value-of select="bom:timestamp"/>
		<xsl:text> \&cr;</xsl:text>
		<xsl:text>Tool: </xsl:text>
		<xsl:value-of select="bom:tools/bom:tool[1]/bom:vendor"/>
		<xsl:text> (</xsl:text>
		<xsl:value-of select="bom:tools/bom:tool[1]/bom:version"/>
		<xsl:text>). &cr;&cr;</xsl:text>
	</xsl:template>

	<xsl:template match="bom:components">
		<xsl:text>## Components &cr;&cr;</xsl:text>
		<xsl:apply-templates select="bom:component"/>
		<xsl:text>&cr;</xsl:text>
	</xsl:template>

	<xsl:template match="bom:component">
		<xsl:text>### </xsl:text>
		<xsl:value-of select="bom:name"/>
		<xsl:text> (</xsl:text>
		<xsl:value-of select="bom:version"/>
		<xsl:text>) &cr;&cr;</xsl:text>

		<xsl:text>Author: </xsl:text>
		<xsl:value-of select="bom:author"/>
		<xsl:text> \&cr;</xsl:text>

		<xsl:if test="bom:copyright">
			<xsl:text>Copyright: </xsl:text>
			<xsl:value-of select="bom:copyright"/>
			<xsl:text> \&cr;</xsl:text>
		</xsl:if>

		<xsl:for-each select="bom:licenses">
			<xsl:choose>
				<xsl:when test="bom:license/bom:id">
					<xsl:text>License: </xsl:text>
					<xsl:value-of select="bom:license/bom:id"/>
					<xsl:text> \&cr;</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>License: [</xsl:text>
					<xsl:value-of select="bom:license/bom:name"/>
					<xsl:text>](</xsl:text>
					<xsl:value-of select="bom:license/bom:url"/>
					<xsl:text>) \&cr;</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>

		<xsl:for-each select="bom:externalReferences">
			<xsl:text>Reference: [</xsl:text>
			<xsl:value-of select="bom:reference/@type"/>
			<xsl:text>](</xsl:text>
			<xsl:value-of select="bom:reference/bom:url"/>
			<xsl:text>) \&cr;</xsl:text>
		</xsl:for-each>

		<xsl:for-each select="bom:hashes">
			<xsl:text>Hash: [</xsl:text>
			<xsl:value-of select="bom:hash/@alg"/>
			<xsl:text>](</xsl:text>
			<xsl:value-of select="bom:hash"/>
			<xsl:text>) \&cr;</xsl:text>
		</xsl:for-each>

		<xsl:text>Package URL: </xsl:text>
		<xsl:value-of select="bom:purl"/>
		<xsl:text> &cr;</xsl:text>

		<xsl:if test="bom:description">
			<xsl:text>&cr;#### Description: &cr;&cr;</xsl:text>
			<xsl:value-of select="bom:description"/>
			<xsl:text> &cr;&cr;</xsl:text>
		</xsl:if>

		<xsl:text>&cr;</xsl:text>
	</xsl:template>

	<!--<components>
		<component type="library" bom-ref="pkg:nuget/AWSSDK.Core@3.7.100.14">
			<author>Amazon Web Services</author>
			<name>AWSSDK.Core</name>
			<version>3.7.100.14</version>
			<description>The Amazon Web Services SDK for .NET - Core Runtime</description>
			<scope>required</scope>
			<hashes>
				<hash alg="SHA-512">5F0EDBAB7784CD0E7231446D23EE80336BAD76EA62952FE98428A28E64B538DD801800F3AD45D429558124B15E2BA16BF054C71E09766EF8F7B06AEAC9D924EB</hash>
			</hashes>
			<licenses>
				<license>
					<name>Unknown - See URL</name>
					<url>http://aws.amazon.com/apache2.0/</url>
				</license>
			</licenses>
			<purl>pkg:nuget/AWSSDK.Core@3.7.100.14</purl>
			<externalReferences>
				<reference type="website">
					<url>https://github.com/aws/aws-sdk-net/</url>
				</reference>
			</externalReferences>
		</component>-->

</xsl:stylesheet>