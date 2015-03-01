<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
    xmlns="http://www.omg.org/spec/BPMN/20100524/MODEL"
    xmlns:atomic="http://www.atomicplatform.com/Process">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/">
    <definitions id="definitions">
      <xsl:for-each select="/atomic:process">
        <process id="{@id}" name="{@name}">
          <startEvent id="_start" />
          <endEvent id="_stop" />
          <xsl:for-each select="atomic:tasks/atomic:task">
            <scriptTask id="{@id}" name="{@name}" scriptFormat="">
              <script>
                <xsl:value-of select="runScript" />
              </script>
            </scriptTask>
          </xsl:for-each>
          <xsl:for-each select="atomic:conditions/atomic:taskCondition">
            <xsl:variable name="flowID">
              <xsl:value-of select="@id" />
            </xsl:variable>
            <xsl:variable name="sourceRef">
              <xsl:value-of select="task/@id" />
            </xsl:variable>
            <xsl:for-each select="/atomic:process/atomic:events/atomic:event">
              <xsl:if test="startOnCondition/@id = $flowID">
                <sequenceFlow id="{$flowID}" sourceRef="{$sourceRef}" targetRef="{@id}" />
              </xsl:if>
            </xsl:for-each>
            <xsl:for-each select="/atomic:process/atomic:tasks/atomic:task">
              <xsl:if test="startOnCondition/@id = $flowID">
                <sequenceFlow id="{$flowID}" sourceRef="{$sourceRef}" targetRef="{@id}" />
              </xsl:if>
            </xsl:for-each>
          </xsl:for-each>
        </process>
      </xsl:for-each>
    </definitions>
    <!--
    <xsl:for-each select="/atomic:app">
      <xsl:for-each select="bpmn:process">
        <atomic:app>
          <process id="{@id}" name="{@name}" />
          <events>
            <xsl:if test="bpmn:sequenceFlow/@targetRef = '_start'">
              <event id="_start">
                <startOnCondition id="wah" />
              </event>
            </xsl:if>
            <xsl:if test="bpmn:sequenceFlow/@targetRef = '_stop'">
              <event id="_stop">
                <xsl:for-each select="bpmn:sequenceFlow[@targetRef = '_stop']">
                  <startOnCondition id="{@id}" />
                </xsl:for-each>
              </event>
            </xsl:if>
          </events>
          <tasks>
            <xsl:for-each select="bpmn:scriptTask">
              <task id="{@id}" name="{@name}">
                <xsl:variable name="taskID">
                  <xsl:value-of select="@id" />
                </xsl:variable>
                <xsl:for-each select="../bpmn:sequenceFlow[@targetRef = $taskID]">
                  <startOnCondition id="{@id}" />
                </xsl:for-each>
                <runScript>
                  <xsl:value-of select="bpmn:script" />
                </runScript>
              </task>
            </xsl:for-each>
          </tasks>
          <conditions>
            <xsl:for-each select="bpmn:sequenceFlow">
              <taskCondition id="{@id}" name="{@name}">
                <task id="{@sourceRef}" />
                <state>Done</state>
              </taskCondition>
            </xsl:for-each>
          </conditions>
        </atomic:app>
      </xsl:for-each>
    </xsl:for-each>
    -->
  </xsl:template>
</xsl:stylesheet>
