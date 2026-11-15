<?xml version="1.0" encoding="utf-8"?>
<!-- Трансформація розкладу (Schedule/Lesson) у HTML-таблицю. -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes" />

  <xsl:template match="/">
    <html>
      <head>
        <title>Розклад занять</title>
        <style>
          body  { font-family: "Segoe UI", Arial, sans-serif; margin: 24px; color: #222; }
          h1    { font-size: 22px; margin-bottom: 4px; }
          p     { color: #666; margin-top: 0; }
          table { border-collapse: collapse; width: 100%; font-size: 14px; }
          th, td { border: 1px solid #c8c8c8; padding: 6px 8px; text-align: left; vertical-align: top; }
          th    { background: #512bd4; color: #fff; }
          tr:nth-child(even) td { background: #f4f1fd; }
          .online { color: #2e7d32; font-style: italic; }
          ul    { margin: 0; padding-left: 18px; }
        </style>
      </head>
      <body>
        <h1>Розклад занять</h1>
        <p>Кількість занять: <xsl:value-of select="count(Schedule/Lesson)" /></p>
        <table>
          <tr>
            <th>День</th>
            <th>Час</th>
            <th>Дисципліна</th>
            <th>Викладач (П.І.П.)</th>
            <th>Факультет</th>
            <th>Кафедра</th>
            <th>Аудиторія</th>
            <th>Група</th>
            <th>Склад студентів</th>
          </tr>
          <xsl:apply-templates select="Schedule/Lesson" />
        </table>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="Lesson">
    <tr>
      <td><xsl:value-of select="@Day" /></td>
      <td><xsl:value-of select="@Time" /></td>
      <td><xsl:value-of select="@Subject" /></td>
      <td><xsl:value-of select="@Teacher" /></td>
      <td><xsl:value-of select="@Faculty" /></td>
      <td><xsl:value-of select="@Department" /></td>
      <td>
        <xsl:choose>
          <xsl:when test="@Room"><xsl:value-of select="@Room" /></xsl:when>
          <xsl:otherwise><span class="online">онлайн</span></xsl:otherwise>
        </xsl:choose>
      </td>
      <td><xsl:value-of select="@Group" /></td>
      <td>
        <ul>
          <xsl:for-each select="Students/Student">
            <li><xsl:value-of select="@Name" /></li>
          </xsl:for-each>
        </ul>
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet>
