const width = $(".col-lg-8").width();
const height = 500;
const margin = { top: width*0.02, right: width*0.03 ,bottom: width*0.08, left: width*0.1 };
const graphWidth = width -  margin.right;
const graphHeight = height - margin.top - margin.bottom;

var format = d3.format(",.2r");
const svg = d3.select(".barchart")
    .append("svg")
    .attr("viewBox", [0, 0, width, height]);
const graph = svg.append("g")
    .attr("width", graphWidth)
    .attr("height", graphHeight)
    .attr("transform", `translate(${margin.left}, ${margin.top})`);
const gXAxis = graph.append("g")
    .attr("transform", `translate(0, ${graphHeight})`);
const gYAxis = graph.append("g");

d3.json("/data/MonthlyRevenueBarChartData.json").then(data => {
    const x = d3.scaleLinear()
        .domain([0, d3.max(data, d => d.Value)])
        .range([0, graphWidth - margin.left - margin.right]);

    const y = d3.scaleBand()
        .domain(data.map(item => item.Date))
        .range([0, graphHeight])
        .paddingInner(0.1)
        .paddingOuter(0.2);

    const rects = graph.selectAll("rect")
        .data(data);

    rects.attr("class", "bar-rect")
        .attr("height", y.bandwidth)
        .attr("width", d =>  (x(d.Value)-x(0)))
        .attr("x", d => x(0))
        .attr("y", d => y(d.Date));

    rects.enter()
        .append("rect")
        .attr("class", "bar-rect")
        .attr("height", y.bandwidth)
        .attr("width", d=> x(d.Value) - x(0))
        .attr("x", d => x(0))
        .attr("y", d => y(d.Date));

    rects.enter()
        .append("text")
        .attr("class", "text-rect")
        .attr("fill", "white")
        .attr("text-anchor", "end")
        .attr("font-family","Roboto")
        .attr("font-size", 12)
        .attr("x", d => x(d.Value))
        .attr("y", (d) => y(d.Date))
        .attr("dx", -4)
        .text(d => format(d.Value)+" $" )
        .call(text => text.filter(d => d.Value > 0) // short bars for values 
            .attr("dx", +3)
            .attr("dy", "0.8em")
            .attr("fill", "black")
            .attr("text-anchor", "start"));


    const xAxis = d3.axisBottom(x)
        .ticks(5)
        .tickFormat(d => `${d}$`);

    const yAxis = d3.axisLeft(y);


    gXAxis.call(xAxis);
    gYAxis.call(yAxis);
    gXAxis.selectAll("text")
        .attr("transform", "rotate(-45)")
        .style("text-anchor", "end")
        .style("font-size", 15);

    gYAxis.selectAll("text")
        .style("font-size", 12).style("text-anchor", "start").style("font-weight", "bold")
        .attr('transform', () => { return 'translate(-90,0)' });
});