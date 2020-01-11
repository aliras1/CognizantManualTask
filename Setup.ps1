./zbctl/zbctl.exe --insecure deploy ./ManualTaskWorkflow.bpmn

for($i=1; $i -le 5;  $i++){
	./zbctl/zbctl.exe --insecure create instance ManualTaskWorkflow --variables '{\"result\": \"FAIL\"}'
}