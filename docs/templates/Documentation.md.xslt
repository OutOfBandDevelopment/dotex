<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE xsl:stylesheet [
	<!ENTITY cr "
">
	<!ENTITY tab "    ">
	<!ENTITY sp " ">
]>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="text" omit-xml-declaration="yes" indent="no" />

	<xsl:template match="/">
		<xsl:apply-templates select="//assembly"/>
	</xsl:template>

	<!-- Assembly template -->
	<xsl:template match="assembly">
		<xsl:text># </xsl:text>
		<xsl:value-of select="name"/>
		<xsl:text>&cr;</xsl:text>
		<xsl:apply-templates select="//member[contains(@name,'T:') and not (descendant::inheritdoc)]"/>
	</xsl:template>

	<!-- Type template -->
	<xsl:template match="//member[contains(@name,'T:') and not (descendant::inheritdoc)]">
		<xsl:variable name="FullMemberName" select="substring-after(@name, ':')"/>
		<xsl:variable name="MemberName">
			<xsl:choose>
				<xsl:when test="contains(@name, '.')">
					<xsl:value-of select="substring-after(@name, '.')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="substring-after(@name, ':')"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:text>&cr;&cr;## Class: </xsl:text>
		<xsl:value-of select="$MemberName"/>

		<xsl:apply-templates />

		<!-- Fields -->
		<xsl:if test="//member[contains(@name,concat('F:',$FullMemberName)) and not (descendant::inheritdoc)]">
			<xsl:text>&cr;### Fields</xsl:text>
			<xsl:text>&cr;</xsl:text>

			<xsl:for-each select="//member[contains(@name,concat('F:',$FullMemberName)) and not (descendant::inheritdoc)]">
				<xsl:text>&cr;#### </xsl:text>
				<xsl:value-of select="substring-after(@name, concat('F:',$FullMemberName,'.'))"/>
				<xsl:text>&cr;</xsl:text>
				<xsl:value-of select="normalize-space()" />
			</xsl:for-each>
		</xsl:if>

		<!-- Properties -->
		<xsl:if test="//member[contains(@name,concat('P:',$FullMemberName)) and not (descendant::inheritdoc)]">
			<xsl:text>&cr;### Properties</xsl:text>
			<xsl:text>&cr;</xsl:text>

			<xsl:for-each select="//member[contains(@name,concat('P:',$FullMemberName)) and not (descendant::inheritdoc)]">
				<xsl:text>&cr;#### </xsl:text>
				<xsl:value-of select="substring-after(@name, concat('P:',$FullMemberName,'.'))"/>
				<xsl:text>&cr;</xsl:text>
				<xsl:value-of select="normalize-space()" />
			</xsl:for-each>
		</xsl:if>

		<!-- Methods -->
		<xsl:if test="//member[contains(@name,concat('M:',$FullMemberName)) and not (descendant::inheritdoc)]">
			<xsl:text>&cr;### Methods</xsl:text>
			<xsl:text>&cr;</xsl:text>

			<xsl:for-each select="//member[contains(@name,concat('M:',$FullMemberName)) and not (descendant::inheritdoc)]">

				<!-- If this is a constructor, display the type name (instead of "#ctor"), or display the method name -->
				<xsl:choose>
					<xsl:when test="contains(@name, '#ctor')">
						<xsl:text>&cr;&cr;#### Constructor</xsl:text>
						<!-- xsl:value-of select="$MemberName"/ -->
						<!-- xsl:value-of select="substring-after(@name, '#ctor')"/-->
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>&cr;&cr;#### </xsl:text>
						<xsl:value-of select="substring-after(@name, concat('M:',$FullMemberName,'.'))"/>
					</xsl:otherwise>
				</xsl:choose>

				<xsl:if test="count(remarks)!=0">
					<xsl:apply-templates select="remarks" />
				</xsl:if>

				<xsl:if test="count(summary)!=0">
					<xsl:apply-templates select="summary" />
				</xsl:if>

				<xsl:if test="count(param)!=0">
					<xsl:text>&cr;&cr;##### Parameters&cr;</xsl:text>
					<xsl:apply-templates select="param"/>
					<xsl:text>&cr;&cr;</xsl:text>
				</xsl:if>

				<xsl:if test="count(returns)!=0">
					<xsl:text>&cr;&cr;##### Return value&cr;</xsl:text>
					<xsl:apply-templates select="returns"/>
					<xsl:text>&cr;&cr;</xsl:text>
				</xsl:if>

				<xsl:if test="count(exception)!=0">
					<xsl:text>&cr;&cr;##### Exceptions&cr;</xsl:text>
					<xsl:apply-templates select="exception"/>
					<xsl:text>&cr;&cr;</xsl:text>
				</xsl:if>

				<xsl:if test="count(example)!=0">
					<xsl:text>&cr;&cr;##### Example&cr;</xsl:text>
					<xsl:text>&cr;</xsl:text>
					<xsl:apply-templates select="example" />
					<xsl:text>&cr;&cr;</xsl:text>
				</xsl:if>

			</xsl:for-each>
		</xsl:if>
	</xsl:template>

	<xsl:template match="summary">
		<xsl:text>&cr;</xsl:text>
		<xsl:apply-templates select="node()" />
	</xsl:template>

	<xsl:template match="remarks">
		<xsl:text>&cr;</xsl:text>
		<xsl:value-of select="normalize-space()" />
	</xsl:template>

	<xsl:template match="c">
		<xsl:text>`</xsl:text>
		<xsl:value-of select="normalize-space()" />
		<xsl:text>`</xsl:text>
	</xsl:template>

	<xsl:template match="code">
		<xsl:text>&cr;```&cr;</xsl:text>
		<xsl:value-of select="text()" />
		<xsl:text>```</xsl:text>
	</xsl:template>

	<xsl:template match="exception">
		<xsl:text>&cr;* *</xsl:text><xsl:value-of select="substring-after(@cref,'T:')"/>:* <xsl:value-of select="normalize-space()" /><xsl:text>&cr;</xsl:text>
	</xsl:template>

	<xsl:template match="include">
		[External file]({@file})
	</xsl:template>

	<xsl:template match="para">
		<xsl:value-of select="normalize-space()" />
	</xsl:template>

	<xsl:template match="param">
		<xsl:if test="not(starts-with(../@name, 'T:'))">
			<xsl:text>* *</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text>:* </xsl:text>
			<xsl:value-of select="normalize-space()" />
			<xsl:text>&cr;</xsl:text>
		</xsl:if>
	</xsl:template>

	<xsl:template match="paramref">
		<xsl:text>*</xsl:text>
		<xsl:value-of select="@name" />
		<xsl:text>*</xsl:text>
	</xsl:template>

	<xsl:template match="permission">
		<xsl:text>*Permission:* *</xsl:text><xsl:value-of select="@cref" />* &cr;<xsl:value-of select="normalize-space()" />
	</xsl:template>

	<xsl:template match="returns">
		<xsl:value-of select="normalize-space()" />
	</xsl:template>

	<xsl:template match="text()">
		<xsl:value-of select="normalize-space()" />
		<xsl:text> &cr;</xsl:text>
	</xsl:template>

	<xsl:template match="see|seealso">
		<xsl:choose>
			<xsl:when test="@href">
				<xsl:text>[</xsl:text>
				<xsl:value-of select="text()" />
				<xsl:text>](</xsl:text>
				<xsl:value-of select="@href" />
				<xsl:text>)</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text> *See: </xsl:text>
				<xsl:value-of select="@cref" />
				<xsl:text>*</xsl:text>				
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>