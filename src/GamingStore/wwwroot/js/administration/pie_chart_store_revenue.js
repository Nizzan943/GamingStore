﻿var format = d3.format(",.2r");
var formatPrec = d3.format(".0%");
d3.json("/data/StoresRevenuePieChart.json").then(data => {
    const size = 900;
    const fourth = size / 4;
    const half = size / 2;
    const labelOffset = fourth * 1.2;
    const total = data.reduce((acc, cur) => acc + cur.Value, 0);
    const container = d3.select(".piechart");

    const chart = container.append('svg')
        .style('width', '100%')
        .attr('viewBox', `0 0 ${size} ${size}`);

    const plotArea = chart.append('g')
        .attr('transform', `translate(${half}, ${half})`);

    const color = d3.scaleOrdinal()
        .domain(data.reverse().map(d => d.name))
        .range(d3.schemeSet1);

    const pie = d3.pie()
        .sort(null)
        .value(d => d.Value);

    const arcs = pie(data);

    const arc = d3.arc()
        .innerRadius(0)
        .outerRadius(half);

    const arcLabel = d3.arc()
        .innerRadius(labelOffset+5)
        .outerRadius(labelOffset);

    plotArea.selectAll('path')
        .data(arcs)
        .enter()
        .append('path')
        .attr('fill', d => color(d.data.Name))
        .attr('stroke', 'white')
        .attr('d', arc);

    const labels = plotArea.selectAll('text')
        .data(arcs)
        .enter()
        .append('text')
        .style('text-anchor', 'middle')
        .style('alignment-baseline', 'middle')
        .style('font-size', '8px')
        .attr('transform', d => `translate(${arcLabel.centroid(d)})`);

    labels.append('tspan')
        .attr('y', '-0.6em')
        .attr('x', 0)
        .style('font-weight', 'bold')
        .text(d => `${d.data.Name}`)
        .call(text => text.filter(d => (d.data.Value / total) < 0.025) // short bars for values 
            .style("font-size", "0px"));

    labels.append('tspan')
        .attr('y', '0.6em')
        .attr('x', 0)
        .text(d => `${format(d.data.Value)}$ (${formatPrec(d.data.Value / total)})`)
        .call(text => text.filter(d => (d.data.Value / total) < 0.03) // short bars for values 
            .style("font-size", "0"));
});