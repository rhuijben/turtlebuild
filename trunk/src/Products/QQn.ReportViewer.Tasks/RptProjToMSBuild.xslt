<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="http://schemas.microsoft.com/developer/msbuild/2003" exclude-result-prefixes="ms" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
	<xsl:output method="xml" indent="yes" />
	<xsl:namespace-alias result-prefix="#default" stylesheet-prefix="ms"/>
	<xsl:template match="/Project">
		<ms:Project DefaultTargets="Build">
			<xsl:call-template name="configurations" />
			<xsl:call-template name="compiles" />
			<ms:Import Project="$(MSBuildExtensionsPath)\QQn\TurtleBuild\1.0\ReportServer.targets" />
			<ms:ProjectExtensions>
				<xsl:copy-of  select="."/>
			</ms:ProjectExtensions>
		</ms:Project>
	</xsl:template>
	<xsl:template match="/ms:Project">
		<xsl:copy-of  select="."/>
	</xsl:template>
	<xsl:template name="configurations">
		<ms:PropertyGroup>			
			<TargetName>ReportServerData</TargetName>
			<ReportServerState>
				<xsl:value-of select="/Project/State"/>
				</ReportServerState>
		</ms:PropertyGroup>
		<xsl:for-each select="/Project/Configurations/Configuration">
			<ms:PropertyGroup>
				<xsl:attribute name="Condition">
					<xsl:text> '$(Configuration)' == '</xsl:text>
					<xsl:value-of select="Name"/>
					<xsl:text>' </xsl:text>
				</xsl:attribute>
				<OutputPath>bin\$(Configuration)\</OutputPath>
				<xsl:for-each select="Options/*">
					<xsl:element name="{local-name()}">
						<xsl:value-of select="."/>
					</xsl:element>
				</xsl:for-each>
			</ms:PropertyGroup>
		</xsl:for-each>
	</xsl:template>
	<xsl:template name="compiles">
		<ms:ItemGroup>
			<xsl:comment>DataSources</xsl:comment>
			<xsl:for-each select="/Project/DataSources/ProjectItem">
				<ms:Content Include="{FullPath}" />
			</xsl:for-each>
		</ms:ItemGroup>
		<ms:ItemGroup>
			<xsl:comment>Reports</xsl:comment>
			<xsl:for-each select="/Project/Reports/ProjectItem">
				<ms:Content Include="{FullPath}" />
			</xsl:for-each>
		</ms:ItemGroup>
	</xsl:template>
</xsl:stylesheet>