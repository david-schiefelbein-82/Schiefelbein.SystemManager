function createSystemInfo(urlSystemInfo) {

    var _urlSystemInfo = urlSystemInfo;
    var _cpuCoreCharts = [];
    var _memoryChart;
    var _diskCharts = {};

    function createCpuCoreGraph(elementId, coreId) {
        var cpuCoreChart = createGraph(elementId, 'cpu ' + coreId);
        _cpuCoreCharts.push(cpuCoreChart);
    }

    function createMemoryGraph(elementId) {
        _memoryChart = createGraph(elementId, 'memory');
    }

    function createDiskGraph(elementId, title) {
        let diskChart = createPieGraph(elementId, title);
        _diskCharts[title] = diskChart;
    }

    /// SUMMARY
    ///        Create a graph on the element
    function createPieGraph(elementId, title) {
        const canvas = document.getElementById(elementId);
        
        const data = {
            labels: ['Used', 'Free'],
            datasets: [
                {
                    label: '',
                    data: [100],
                    backgroundColor: ['rgb(200, 200, 200)'],
                }
            ]
        };
        
        const config = {
            type: 'pie',
            data: data,
            options: {
                maintainAspectRatio: false,
                legend: {
                    display: false
                },
                animation: {
                    duration: 50, // general animation time
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: title
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return printBytes(context.raw);
                            }
                        }
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
    ///        Create a graph on the element
    function createGraph(elementId, label) {
        const canvas = document.getElementById(elementId);

        var startLabels = [];
        var startData = [];

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
    function initSystem(cpuCoreCount, cpuCoreSelectors, memorySelector, diskSelector, callback) {
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

            let disks = response.disk;
            if (disks !== null && disks !== undefined) {
                for (disk of disks) {
                    let chart = _diskCharts[disk.name];
                    if (chart !== null && chart !== undefined)
                        initDiskData(chart, disk);
                }
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

            let disks = response.disk;
            if (disks !== null && disks !== undefined) {
                for (disk of disks) {
                    var chart = _diskCharts[disk.name];
                    if (chart !== null && chart !== undefined) {
                        addDiskData(_diskCharts[disk.name], disk);
                    }
                }
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

    function initDiskData(chart, diskData) {
        let freeSpace = diskData.freeSpace[diskData.freeSpace.length - 1];
        let total = diskData.totalSize;
        let usedSpace = total - freeSpace;
        var name = diskData.name;
        var driveFormat = diskData.driveFormat;
        var driveType = diskData.driveType;
        if (driveType !== undefined && driveType != null) {
            if (driveType == "Fixed") {
                chart.options.plugins.title.text = name + " " + driveFormat;
            } else {
                chart.options.plugins.title.text = name + " " + driveType;
            }
        }
        chart.data.datasets.forEach((dataset) => {
            dataset.data = [usedSpace, freeSpace]
            dataset.backgroundColor = ['rgb(255, 128, 128)', 'rgb(128, 255, 128)'];
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

    function printBytes(bytes) {
        let MB = 1024.0 * 1024.0;
        let GB = MB * 1024.0;
        let TB = GB * 1024.0;
        if (bytes >= TB) {
            var tb = bytes / TB;
            return " " + (Math.round(tb * 100) / 100.0).toLocaleString() + " TB";
        }

        var gb = bytes / GB;
        return " " + (Math.round(gb * 10) / 10.0).toLocaleString() + " GB";
    }

    function addDiskData(chart, diskData) {
        let freeSpace = diskData.freeSpace[diskData.freeSpace.length - 1];
        let total = diskData.totalSize;
        let usedSpace = total - freeSpace;
        chart.data.datasets.forEach((dataset) => {
            dataset.data = [ usedSpace, freeSpace ];
        });
        chart.update();
    }

    var obj = {
        addData: addData,
        createCpuCoreGraph: createCpuCoreGraph,
        createMemoryGraph: createMemoryGraph,
        createDiskGraph: createDiskGraph,
        init: initSystem,
        refresh: refresh,
    };
    return obj;
}