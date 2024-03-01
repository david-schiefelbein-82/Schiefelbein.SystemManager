function createSystemInfo(urlSystemInfo) {

    var _urlSystemInfo = urlSystemInfo;
    var _cpuCoreCharts = [];
    var _memoryChart;

    function createCpuCoreGraph(elementId, coreId) {
        var cpuCoreChart = createGraph(elementId, 'cpu ' + coreId);
        _cpuCoreCharts.push(cpuCoreChart);
    }

    function createMemoryGraph(elementId) {
        _memoryChart = createGraph(elementId, 'memory');
    }

    /// SUMMARY
    ///        Create a graph on the element
    function createGraph(elementId, label) {
        const canvas = document.getElementById(elementId);

        var startLabels = [];
        var startData = [];

        //for (var i = 0; i < 60; ++i) {
        //    startLabels.push('');
        //    startData.push({ x: i, y: 0 });
        //}

        const data = {
            labels: startLabels,
            datasets: [{
                label: label,
                pointRadius: 0,
                backgroundColor: 'rgb(241, 246, 250)',
                borderColor: 'rgb(17, 125, 187)',
                borderWidth: 1,
                fill: 'origin',
                data: startData,
            }]
        };

        const config = {
            type: 'line',
            data: data,
            options: {
                maintainAspectRatio: false,
                legend: {
                    display: false
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        }
                    },
                    y: {
                        min: 0,
                        max: 100,
                        grid: {
                            display: true
                        },
                        ticks: {
                            stepSize: 20
                        }
                    }
                },
                animation: {
                    duration: 50, // general animation time
                },
                plugins: {
                    legend: {
                        display: false
                    }
                }
            }
        };

        const jsChart = new Chart(
            canvas,
            config
        );

        return jsChart;
    }


    /// SUMMARY
    ///         : retrieve system info stats and update the graphs
    ///
    function initSystem(cpuCoreCount, cpuCoreSelectors, memorySelector, callback) {
        var start = performance.now();
        $.ajax({
            url: _urlSystemInfo,
        }).then(function (response) {
            var delta = performance.now() - start;
            console.log("<<<<<<<<        initSystem succeeded after " + delta + "ms");
            let cores = response.cpu.cores;
            for (var coreId = 0; coreId < cores.length && coreId < _cpuCoreCharts.length; coreId++) {
                let values = cores[coreId];
                initData(_cpuCoreCharts[coreId], values);
            }

            let memory = response.memory.percentUtilised;
            if (memory.length > 0) {
                initData(_memoryChart, memory);
            }

            callback(true);
        }).catch(function (jqXHR, textStatus, errorThrown) {
            var delta = performance.now() - start;
            console.log("<<<<<<<<        initSystem failed after " + delta + " ms");
            var text = jqXHR.responseText;
            callback(false, text);
        });
    }


    /// SUMMARY
    ///         : retrieve system info stats and update the graphs
    ///
    function refresh(callback) {
        // add new data
        $.ajax({
            url: _urlSystemInfo,
        }).then(function (response) {
            console.log("ajax succeeded");
            let cores = response.cpu.cores;
            for (var coreId = 0; coreId < cores.length && coreId < _cpuCoreCharts.length; coreId++) {
                let values = cores[coreId];
                if (values.length > 0)
                    addData(_cpuCoreCharts[coreId], values[values.length - 1]);
            }

            let memory = response.memory.percentUtilised;
            if (memory.length > 0) {
                addData(_memoryChart, memory[memory.length - 1]);
            }

            callback(true);
        }).catch(function (jqXHR, textStatus, errorThrown) {
            var text = jqXHR.responseText;
            callback(false, text);
        });
    }

    function initData(chart, values) {
        chart.data.datasets.forEach((dataset) => {
            var index = 0;
            while ((values.length + index) < 60) {
                chart.data.labels.push('');
                dataset.data.push({ x: index, y: 0 });
                index++;
            }

            for (var i = 0; i < values.length; ++i) {
                chart.data.labels.push('');
                dataset.data.push({ x: index, y: values[i] });
                index++;
            }
        });
        chart.update();
    }

    /// SUMMARY
    ///         : function to add a point of data to an existing chart
    ///
    function addData(chart, value) {
        chart.data.labels.shift();
        chart.data.labels.push('');
        chart.data.datasets.forEach((dataset) => {
            dataset.data.shift();
            dataset.data.push({ x: 60, y: value });
        });
        chart.update();
    }

    var obj = {
        addData: addData,
        createCpuCoreGraph: createCpuCoreGraph,
        createMemoryGraph: createMemoryGraph,
        init: initSystem,
        refresh: refresh,
    };
    return obj;
}