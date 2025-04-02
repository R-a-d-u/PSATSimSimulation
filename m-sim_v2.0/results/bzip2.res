sim-outorder: SimpleScalar/Alpha Tool Set version 3.0 of August, 2003.
Copyright (c) 1994-2003 by Todd M. Austin, Ph.D. and SimpleScalar, LLC.
All Rights Reserved. This version of SimpleScalar is licensed for academic
non-commercial use.  No portion of this work may be used by any commercial
entity, or for any commercial purpose, without the prior written permission
of SimpleScalar, LLC (info@simplescalar.com).


Processor Parameters:
Issue Width: 4
Window Size: 128
Number of Virtual Registers: 32
Number of Physical Registers: 128
Datapath Width: 64
Total Power Consumption: 95.1592
Branch Predictor Power Consumption: 4.52313  (4.86%)
 branch target buffer power (W): 4.16837
 local predict power (W): 0.0879711
 global predict power (W): 0.0996078
 chooser power (W): 0.0702439
 RAS power (W): 0.0969383
Rename Logic Power Consumption: 0.616985  (0.662%)
 Instruction Decode Power (W): 0.0159915
 RAT decode_power (W): 0.113514
 RAT wordline_power (W): 0.0447235
 RAT bitline_power (W): 0.431848
 DCL Comparators (W): 0.0109075
Instruction Window Power Consumption: 8.19411  (8.8%)
 tagdrive (W): 0.277708
 tagmatch (W): 0.106989
 Selection Logic (W): 0.0295277
 decode_power (W): 0.543666
 wordline_power (W): 0.0844643
 bitline_power (W): 7.15176
Load/Store Queue Power Consumption: 3.73642  (4.01%)
 tagdrive (W): 1.55613
 tagmatch (W): 0.604642
 decode_power (W): 0.0616683
 wordline_power (W): 0.0150119
 bitline_power (W): 1.49897
Arch. Register File Power Consumption: 3.57247  (3.83%)
 decode_power (W): 0.113514
 wordline_power (W): 0.0844643
 bitline_power (W): 3.37449
Result Bus Power Consumption: 5.42703  (5.83%)
Total Clock Power: 29.4661  (31.6%)
Int ALU Power: 4.66013  (5%)
FP ALU Power: 14.281  (15.3%)
Instruction Cache Power Consumption: 3.97049  (4.26%)
 decode_power (W): 1.67268
 wordline_power (W): 0.028656
 bitline_power (W): 1.5122
 senseamp_power (W): 0.096
 tagarray_power (W): 0.660946
Itlb_power (W): 0.263317 (0.283%)
Data Cache Power Consumption: 8.34726  (8.96%)
 decode_power (W): 1.44978
 wordline_power (W): 0.196735
 bitline_power (W): 4.58765
 senseamp_power (W): 0.768
 tagarray_power (W): 1.34509
Dtlb_power (W): 0.900515 (0.967%)
Level 2 Cache Power Consumption: 5.20014 (5.58%)
 decode_power (W): 0.41817
 wordline_power (W): 0.0430878
 bitline_power (W): 3.0244
 senseamp_power (W): 0.192
 tagarray_power (W): 1.52247
sim-main: initalizing context 0:bzip2.arg
args: 0:../spec2000binaries/bzip2/bzip200.peak.ev6 1:../spec2000binaries/bzip2/input.source 2:58 
sim: loading binary...
sim: loading ../spec2000binaries/bzip2/bzip200.peak.ev6
warning: section `.comment' ignored...
sim: command line: ./sim-outorder -redir:sim ./results/bzip2.res -fastfwd 2000000 -max:inst 5000000 bzip2.arg 

sim: simulation started @ Tue Apr  1 08:35:16 2025, options follow:

sim-outorder: This simulator implements a very detailed out-of-order issue
superscalar processor with a two-level memory system and speculative
execution support.  This simulator is a performance simulator, tracking the
latency of all pipeline operations.

# -config                     # load configuration from a file
# -dumpconfig                 # dump configuration to a file
# -h                    false # print help message    
# -v                    false # verbose operation     
# -d                    false # enable debug message  
# -i                    false # start in Dlite debugger
-seed                       1 # random number generator seed (0 for timer seed)
# -q                    false # initialize and terminate immediately
# -redir:sim     ./results/bzip2.res # redirect simulator output to file (non-interactive only)
# -redir:prog          <null> # redirect simulated program output to file
-nice                       0 # simulator scheduling priority
-max:inst             5000000 # maximum number of inst's to execute
-fastfwd              2000000 # number of insts skipped before timing starts
# -ptrace              <null> # generate pipetrace, i.e., <fname|stdout|stderr> <range>
-fetch:speed                1 # speed of front-end of machine relative to execution core
-bpred                  bimod # branch predictor type {nottaken|taken|perfect|bimod|2lev|comb}
-bpred:bimod     2048 # bimodal predictor config (<table size>)
-bpred:2lev      1 1024 8 0 # 2-level predictor config (<l1size> <l2size> <hist_size> <xor>)
-bpred:comb      1024 # combining predictor config (<meta_table_size>)
-bpred:ras                  8 # return address stack size (0 for no return stack)
-bpred:btb       512 4 # BTB config (<num_sets> <associativity>)
# -bpred:spec_update       <null> # speculative predictors update in {ID|WB} (default non-spec)
-cpred                  bimod # cache load-latency predictor type {nottaken|taken|perfect|bimod|2lev|comb}
-cpred:bimod     2048 # cache load-latency bimodal predictor config (<table size>)
-cpred:2lev      1 1024 8 0 # cache load-latency 2-level predictor config (<l1size> <l2size> <hist_size> <xor>)
-cpred:comb      1024 # cache load-latency combining predictor config (<meta_table_size>)
-cpred:ras                  0 # return address stack size (0 for no return stack)
-cpred:btb       512 4 # cache load-latency BTB config (<num_sets> <associativity>)
-decode:width               4 # instruction decode B/W (insts/cycle)
-issue:width                4 # instruction issue B/W (insts/cycle)
-issue:inorder          false # run pipeline with in-order issue
-issue:wrongpath         true # issue instructions down wrong execution paths
-commit:width               4 # instruction commit B/W (insts/cycle)
-rob:size                 128 # reorder buffer (ROB) size
-fetch:policy |icount|round_robin|       icount # fetch policy          
-recovery:model        squash # Alpha squash recovery or perfect predition: |squash|perfect|
-iq:size                   32 # issue queue (IQ) size 
-iq:issue_exec_delay            1 # minimum cycles between issue and execution
-fetch_rename_delay            4 # number of cycles between fetch and rename stages
-rename_dispatch_delay            1 # number cycles between rename and dispatch stages
-rf:size                  128 # register file (RF) size for each the INT and FP physical register file)
-lsq:size                  48 # load/store queue (LSQ) size
-cache:dl1       dl1:256:32:4:l # l1 data cache config, i.e., {<config>|none}
-cache:dl1lat               1 # l1 data cache hit latency (in cycles)
-cache:dl2       ul2:1024:64:8:l # l2 data cache config, i.e., {<config>|none}
-cache:dl2lat               6 # l2 data cache hit latency (in cycles)
-cache:il1       il1:512:32:2:l # l1 inst cache config, i.e., {<config>|dl1|dl2|none}
-cache:il1lat               1 # l1 instruction cache hit latency (in cycles)
-cache:il2                dl2 # l2 instruction cache config, i.e., {<config>|dl2|none}
-cache:il2lat               6 # l2 instruction cache hit latency (in cycles)
-cache:flush            false # flush caches on system calls
-cache:icompress        false # convert 64-bit inst addresses to 32-bit inst equivalents
-mem:lat         100 2 # memory access latency (<first_chunk> <inter_chunk>)
-mem:width                  8 # memory access bus width (in bytes)
-tlb:itlb        itlb:16:4096:4:l # instruction TLB config, i.e., {<config>|none}
-tlb:dtlb        dtlb:32:4096:4:l # data TLB config, i.e., {<config>|none}
-tlb:lat                   30 # inst/data TLB miss latency (in cycles)
-res:ialu                   4 # total number of integer ALU's available
-res:imult                  1 # total number of integer multiplier/dividers available
-res:memport                2 # total number of memory system ports available (to CPU)
-res:fpalu                  4 # total number of floating point ALU's available
-res:fpmult                 1 # total number of floating point multiplier/dividers available
# -pcstat              <null> # profile stat(s) against text addr's (mult uses ok)
-power:print_stats         true # print power statistics collected by wattch?

  Pipetrace range arguments are formatted as follows:

    {{@|#}<start>}:{{@|#|+}<end>}

  Both ends of the range are optional, if neither are specified, the entire
  execution is traced.  Ranges that start with a `@' designate an address
  range to be traced, those that start with an `#' designate a cycle count
  range.  All other range values represent an instruction count range.  The
  second argument, if specified with a `+', indicates a value relative
  to the first argument, e.g., 1000:+100 == 1000:1100.  Program symbols may
  be used in all contexts.

    Examples:   -ptrace FOO.trc #0:#1000
                -ptrace BAR.trc @2000:
                -ptrace BLAH.trc :1500
                -ptrace UXXE.trc :
                -ptrace FOOBAR.trc @main:+278

  Branch predictor configuration examples for 2-level predictor:
    Configurations:   N, M, W, X
      N   # entries in first level (# of shift register(s))
      W   width of shift register(s)
      M   # entries in 2nd level (# of counters, or other FSM)
      X   (yes-1/no-0) xor history and address for 2nd level index
    Sample predictors:
      GAg     : 1, W, 2^W, 0
      GAp     : 1, W, M (M > 2^W), 0
      PAg     : N, W, 2^W, 0
      PAp     : N, W, M (M == 2^(N+W)), 0
      gshare  : 1, W, 2^W, 1
  Predictor `comb' combines a bimodal and a 2-level predictor.

  The cache config parameter <config> has the following format:

    <name>:<nsets>:<bsize>:<assoc>:<repl>

    <name>   - name of the cache being defined
    <nsets>  - number of sets in the cache
    <bsize>  - block size of the cache
    <assoc>  - associativity of the cache
    <repl>   - block replacement strategy, 'l'-LRU, 'f'-FIFO, 'r'-random

    Examples:   -cache:dl1 dl1:4096:32:1:l
                -dtlb dtlb:128:4096:32:r

  Cache levels can be unified by pointing a level of the instruction cache
  hierarchy at the data cache hiearchy using the "dl1" and "dl2" cache
  configuration arguments.  Most sensible combinations are supported, e.g.,

    A unified l2 cache (il2 is pointed at dl2):
      -cache:il1 il1:128:64:1:l -cache:il2 dl2
      -cache:dl1 dl1:256:32:1:l -cache:dl2 ul2:1024:64:2:l

    Or, a fully unified cache hierarchy (il1 pointed at dl1):
      -cache:il1 dl1
      -cache:dl1 ul1:256:32:1:l -cache:dl2 ul2:1024:64:2:l



sim: ** fast forwarding insts **
sim: ** starting performance simulation **

sim: ** simulation statistics **
sim_num_insn                5000069 # total number of instructions committed
sim_num_insn 0              5000003 # total number of instructions committed for this thread
sim_num_refs                3587008 # total number of loads and stores committed
sim_num_loads               2717432 # total number of loads committed
sim_num_stores          869576.0000 # total number of stores committed
sim_num_branches             108697 # total number of branches committed
sim_elapsed_time                 13 # total simulation time in seconds
sim_inst_rate           384620.6923 # simulation speed (in insts/sec)
sim_total_insn              5000069 # total number of instructions executed
sim_total_refs              3587008 # total number of loads and stores executed
sim_total_loads             2717432 # total number of loads executed
sim_total_stores        869576.0000 # total number of stores executed
sim_total_branches           108697 # total number of branches executed
sim_cycle                   7010920 # total simulation time in cycles
sim_IPC                      0.7132 # instructions per cycle
sim_CPI                      1.4022 # cycles per instruction
sim_exec_BW                  0.7132 # total instructions (mis-spec + committed) per cycle
sim_IPB                     46.0001 # instruction per branch
sim_slip                  807502637 # total number of slip cycles
avg_sim_slip               161.4983 # the average slip between issue and retirement
Thread_0_bpred_bimod.lookups       108697 # total number of bpred lookups
Thread_0_bpred_bimod.updates       108695 # total number of updates
Thread_0_bpred_bimod.addr_hits       108695 # total number of address-predicted hits
Thread_0_bpred_bimod.dir_hits       108695 # total number of direction-predicted hits (includes addr-hits)
Thread_0_bpred_bimod.misses            0 # total number of misses
Thread_0_bpred_bimod.jr_hits            0 # total number of address-predicted hits for JR's
Thread_0_bpred_bimod.jr_seen            0 # total number of JR's seen
Thread_0_bpred_bimod.jr_non_ras_hits.PP           10 # total number of address-predicted hits for non-RAS JR's
Thread_0_bpred_bimod.jr_non_ras_seen.PP           27 # total number of non-RAS JR's seen
Thread_0_bpred_bimod.bpred_addr_rate    1.0000 # branch address-prediction rate (i.e., addr-hits/updates)
Thread_0_bpred_bimod.bpred_dir_rate    1.0000 # branch direction-prediction rate (i.e., all-hits/updates)
Thread_0_bpred_bimod.bpred_jr_rate <error: divide by zero> # JR address-prediction rate (i.e., JR addr-hits/JRs seen)
Thread_0_bpred_bimod.bpred_jr_non_ras_rate.PP    0.3704 # non-RAS JR addr-pred rate (ie, non-RAS JR hits/JRs seen)
Thread_0_bpred_bimod.retstack_pushes            0 # total number of address pushed onto ret-addr stack
Thread_0_bpred_bimod.retstack_pops            0 # total number of address popped off of ret-addr stack
Thread_0_bpred_bimod.used_ras.PP            0 # total number of RAS predictions used
Thread_0_bpred_bimod.ras_hits.PP            0 # total number of RAS hits
Thread_0_bpred_bimod.ras_rate.PP <error: divide by zero> # RAS prediction rate (i.e., RAS hits/used RAS)
Thread_0_cpred_bimod.lookups       869575 # total number of bpred lookups
Thread_0_cpred_bimod.updates       869575 # total number of updates
Thread_0_cpred_bimod.addr_hits       868551 # total number of address-predicted hits
Thread_0_cpred_bimod.dir_hits       868551 # total number of direction-predicted hits (includes addr-hits)
Thread_0_cpred_bimod.misses         1024 # total number of misses
Thread_0_cpred_bimod.jr_hits            0 # total number of address-predicted hits for JR's
Thread_0_cpred_bimod.jr_seen            0 # total number of JR's seen
Thread_0_cpred_bimod.jr_non_ras_hits.PP            0 # total number of address-predicted hits for non-RAS JR's
Thread_0_cpred_bimod.jr_non_ras_seen.PP            0 # total number of non-RAS JR's seen
Thread_0_cpred_bimod.bpred_addr_rate    0.9988 # branch address-prediction rate (i.e., addr-hits/updates)
Thread_0_cpred_bimod.bpred_dir_rate    0.9988 # branch direction-prediction rate (i.e., all-hits/updates)
Thread_0_cpred_bimod.bpred_jr_rate <error: divide by zero> # JR address-prediction rate (i.e., JR addr-hits/JRs seen)
Thread_0_cpred_bimod.bpred_jr_non_ras_rate.PP <error: divide by zero> # non-RAS JR addr-pred rate (ie, non-RAS JR hits/JRs seen)
Thread_0_cpred_bimod.retstack_pushes            0 # total number of address pushed onto ret-addr stack
Thread_0_cpred_bimod.retstack_pops            0 # total number of address popped off of ret-addr stack
Thread_0_cpred_bimod.used_ras.PP            0 # total number of RAS predictions used
Thread_0_cpred_bimod.ras_hits.PP            0 # total number of RAS hits
Thread_0_cpred_bimod.ras_rate.PP <error: divide by zero> # RAS prediction rate (i.e., RAS hits/used RAS)
il1.accesses                5000085 # total number of accesses
il1.hits                    5000085 # total number of hits
il1.misses                        0 # total number of misses
il1.replacements                  0 # total number of replacements
il1.writebacks                    0 # total number of writebacks
il1.invalidations                 0 # total number of invalidations
il1.miss_rate                0.0000 # miss rate (i.e., misses/ref)
il1.repl_rate                0.0000 # replacement rate (i.e., repls/ref)
il1.wb_rate                  0.0000 # writeback rate (i.e., wrbks/ref)
il1.inv_rate                 0.0000 # invalidation rate (i.e., invs/ref)
dl1.accesses                3586988 # total number of accesses
dl1.hits                    3152203 # total number of hits
dl1.misses                   434785 # total number of misses
dl1.replacements             434785 # total number of replacements
dl1.writebacks               217392 # total number of writebacks
dl1.invalidations                 0 # total number of invalidations
dl1.miss_rate                0.1212 # miss rate (i.e., misses/ref)
dl1.repl_rate                0.1212 # replacement rate (i.e., repls/ref)
dl1.wb_rate                  0.0606 # writeback rate (i.e., wrbks/ref)
dl1.inv_rate                 0.0000 # invalidation rate (i.e., invs/ref)
ul2.accesses                 652177 # total number of accesses
ul2.hits                     434785 # total number of hits
ul2.misses                   217392 # total number of misses
ul2.replacements             217392 # total number of replacements
ul2.writebacks               108695 # total number of writebacks
ul2.invalidations                 0 # total number of invalidations
ul2.miss_rate                0.3333 # miss rate (i.e., misses/ref)
ul2.repl_rate                0.3333 # replacement rate (i.e., repls/ref)
ul2.wb_rate                  0.1667 # writeback rate (i.e., wrbks/ref)
ul2.inv_rate                 0.0000 # invalidation rate (i.e., invs/ref)
itlb.accesses               5000085 # total number of accesses
itlb.hits                   5000085 # total number of hits
itlb.misses                       0 # total number of misses
itlb.replacements                 0 # total number of replacements
itlb.writebacks                   0 # total number of writebacks
itlb.invalidations                0 # total number of invalidations
itlb.miss_rate               0.0000 # miss rate (i.e., misses/ref)
itlb.repl_rate               0.0000 # replacement rate (i.e., repls/ref)
itlb.wb_rate                 0.0000 # writeback rate (i.e., wrbks/ref)
itlb.inv_rate                0.0000 # invalidation rate (i.e., invs/ref)
dtlb.accesses               3586988 # total number of accesses
dtlb.hits                   3583591 # total number of hits
dtlb.misses                    3397 # total number of misses
dtlb.replacements              3397 # total number of replacements
dtlb.writebacks                   0 # total number of writebacks
dtlb.invalidations                0 # total number of invalidations
dtlb.miss_rate               0.0009 # miss rate (i.e., misses/ref)
dtlb.repl_rate               0.0009 # replacement rate (i.e., repls/ref)
dtlb.wb_rate                 0.0000 # writeback rate (i.e., wrbks/ref)
dtlb.inv_rate                0.0000 # invalidation rate (i.e., invs/ref)
sim_invalid_addrs                 0 # total non-speculative bogus addresses seen (debug var)
ld_text_base           0x0120000000 # program text (code) segment base
mem->ld_text_size            196608 # program text (code) size in bytes
mem->ld_data_base      0x0140000000 # program initialized data segment base
mem->ld_data_size            146544 # program init'ed `.data' and uninit'ed `.bss' size in bytes
mem->ld_stack_base     0x011ff9b000 # program stack segment base (highest address in stack)
mem->ld_stack_size            16384 # program initial stack size
mem->ld_prog_entry     0x0120008c30 # program entry point (initial PC)
mem->ld_environ_base   0x011ff97000 # program environment base address address
mem->ld_target_big_endian            0 # target executable endian-ness, non-zero if big endian
mem_0.page_count              22310 # total number of pages allocated
mem_0.page_mem              178480k # total size of memory pages allocated
mem_0.ptab_misses             22355 # total first level page table misses
mem_0.ptab_accesses       107027644 # total page table accesses
mem_0.ptab_miss_rate         0.0002 # first level page table miss rate
rename_power           4325635.0535 # total power usage of rename unit
bpred_power            31711277.3157 # total power usage of bpred unit
window_power           57448270.5138 # total power usage of instruction window
lsq_power              26195761.1165 # total power usage of load/store queue
regfile_power          25046316.1350 # total power usage of arch. regfile
icache_power           29682847.2546 # total power usage of icache
dcache_power           64835413.4292 # total power usage of dcache
dcache2_power          36457736.8758 # total power usage of dcache2
alu_power              132795065.4442 # total power usage of alu
falu_power             100123263.6272 # total power usage of falu
resultbus_power        38048481.2222 # total power usage of resultbus
clock_power            206584763.0722 # total power usage of clock
avg_rename_power             0.6170 # avg power usage of rename unit
avg_bpred_power              4.5231 # avg power usage of bpred unit
avg_window_power             8.1941 # avg power usage of instruction window
avg_lsq_power                3.7364 # avg power usage of lsq
avg_regfile_power            3.5725 # avg power usage of arch. regfile
avg_icache_power             4.2338 # avg power usage of icache
avg_dcache_power             9.2478 # avg power usage of dcache
avg_dcache2_power            5.2001 # avg power usage of dcache2
avg_alu_power               18.9412 # avg power usage of alu
avg_falu_power              14.2810 # avg power usage of falu
avg_resultbus_power          5.4270 # avg power usage of resultbus
avg_clock_power             29.4661 # avg power usage of clock
fetch_stage_power      61394124.5703 # total power usage of fetch stage
dispatch_stage_power   4325635.0535 # total power usage of dispatch stage
issue_stage_power      355780728.6017 # total power usage of issue stage
avg_fetch_power              8.7569 # average power of fetch unit per cycle
avg_dispatch_power           0.6170 # average power of dispatch unit per cycle
avg_issue_power             50.7467 # average power of issue unit per cycle
total_power            653131567.4327 # total power per cycle
avg_total_power_cycle       93.1592 # average total power per cycle
avg_total_power_cycle_nofp_nod2      73.6780 # average total power per cycle
avg_total_power_insn       130.6245 # average total power per insn
avg_total_power_insn_nofp_nod2     103.3087 # average total power per insn
rename_power_cc1       1005962.5586 # total power usage of rename unit_cc1
bpred_power_cc1         491641.2236 # total power usage of bpred unit_cc1
window_power_cc1       22682689.0672 # total power usage of instruction window_cc1
lsq_power_cc1          4830329.7773 # total power usage of lsq_cc1
regfile_power_cc1      4458152.2854 # total power usage of arch. regfile_cc1
icache_power_cc1       6903015.2029 # total power usage of icache_cc1
dcache_power_cc1       22114325.5044 # total power usage of dcache_cc1
dcache2_power_cc1      1697930.1779 # total power usage of dcache2_cc1
alu_power_cc1          10637353.5830 # total power usage of alu_cc1
resultbus_power_cc1    11883589.3846 # total power usage of resultbus_cc1
clock_power_cc1        42838133.2071 # total power usage of clock_cc1
avg_rename_power_cc1         0.1435 # avg power usage of rename unit_cc1
avg_bpred_power_cc1          0.0701 # avg power usage of bpred unit_cc1
avg_window_power_cc1         3.2353 # avg power usage of instruction window_cc1
avg_lsq_power_cc1            0.6890 # avg power usage of lsq_cc1
avg_regfile_power_cc1        0.6359 # avg power usage of arch. regfile_cc1
avg_icache_power_cc1         0.9846 # avg power usage of icache_cc1
avg_dcache_power_cc1         3.1543 # avg power usage of dcache_cc1
avg_dcache2_power_cc1        0.2422 # avg power usage of dcache2_cc1
avg_alu_power_cc1            1.5173 # avg power usage of alu_cc1
avg_resultbus_power_cc1       1.6950 # avg power usage of resultbus_cc1
avg_clock_power_cc1          6.1102 # avg power usage of clock_cc1
fetch_stage_power_cc1  7394656.4265 # total power usage of fetch stage_cc1
dispatch_stage_power_cc1 1005962.5586 # total power usage of dispatch stage_cc1
issue_stage_power_cc1  73846217.4944 # total power usage of issue stage_cc1
avg_fetch_power_cc1          1.0547 # average power of fetch unit per cycle_cc1
avg_dispatch_power_cc1       0.1435 # average power of dispatch unit per cycle_cc1
avg_issue_power_cc1         10.5330 # average power of issue unit per cycle_cc1
total_power_cycle_cc1  129543121.9721 # total power per cycle_cc1
avg_total_power_cycle_cc1      18.4773 # average total power per cycle_cc1
avg_total_power_insn_cc1      25.9083 # average total power per insn_cc1
rename_power_cc2        771242.3525 # total power usage of rename unit_cc2
bpred_power_cc2         245820.6118 # total power usage of bpred unit_cc2
window_power_cc2       19420411.5548 # total power usage of instruction window_cc2
lsq_power_cc2          3354655.5745 # total power usage of lsq_cc2
regfile_power_cc2      1261990.4200 # total power usage of arch. regfile_cc2
icache_power_cc2       6903015.2029 # total power usage of icache_cc2
dcache_power_cc2       16585829.6702 # total power usage of dcache_cc2
dcache2_power_cc2      1695704.5197 # total power usage of dcache2_cc2
alu_power_cc2          5698595.0919 # total power usage of alu_cc2
resultbus_power_cc2    9581334.9017 # total power usage of resultbus_cc2
clock_power_cc2        32164403.3291 # total power usage of clock_cc2
avg_rename_power_cc2         0.1100 # avg power usage of rename unit_cc2
avg_bpred_power_cc2          0.0351 # avg power usage of bpred unit_cc2
avg_window_power_cc2         2.7700 # avg power usage of instruction window_cc2
avg_lsq_power_cc2            0.4785 # avg power usage of instruction lsq_cc2
avg_regfile_power_cc2        0.1800 # avg power usage of arch. regfile_cc2
avg_icache_power_cc2         0.9846 # avg power usage of icache_cc2
avg_dcache_power_cc2         2.3657 # avg power usage of dcache_cc2
avg_dcache2_power_cc2        0.2419 # avg power usage of dcache2_cc2
avg_alu_power_cc2            0.8128 # avg power usage of alu_cc2
avg_resultbus_power_cc2       1.3666 # avg power usage of resultbus_cc2
avg_clock_power_cc2          4.5878 # avg power usage of clock_cc2
fetch_stage_power_cc2  7148835.8147 # total power usage of fetch stage_cc2
dispatch_stage_power_cc2  771242.3525 # total power usage of dispatch stage_cc2
issue_stage_power_cc2  56336531.3128 # total power usage of issue stage_cc2
avg_fetch_power_cc2          1.0197 # average power of fetch unit per cycle_cc2
avg_dispatch_power_cc2       0.1100 # average power of dispatch unit per cycle_cc2
avg_issue_power_cc2          8.0355 # average power of issue unit per cycle_cc2
total_power_cycle_cc2  97683003.2291 # total power per cycle_cc2
avg_total_power_cycle_cc2      13.9330 # average total power per cycle_cc2
avg_total_power_insn_cc2      19.5363 # average total power per insn_cc2
rename_power_cc3       1103209.6019 # total power usage of rename unit_cc3
bpred_power_cc3        3367784.2204 # total power usage of bpred unit_cc3
window_power_cc3       22912197.3453 # total power usage of instruction window_cc3
lsq_power_cc3          5437942.4800 # total power usage of lsq_cc3
regfile_power_cc3      3261816.0061 # total power usage of arch. regfile_cc3
icache_power_cc3       9180998.4067 # total power usage of icache_cc3
dcache_power_cc3       21058977.7007 # total power usage of dcache_cc3
dcache2_power_cc3      5171905.9351 # total power usage of dcache2_cc3
alu_power_cc3          17914366.2766 # total power usage of alu_cc3
resultbus_power_cc3    12117897.7827 # total power usage of resultbus_cc3
clock_power_cc3        48552833.7406 # total power usage of clock_cc3
avg_rename_power_cc3         0.1574 # avg power usage of rename unit_cc3
avg_bpred_power_cc3          0.4804 # avg power usage of bpred unit_cc3
avg_window_power_cc3         3.2681 # avg power usage of instruction window_cc3
avg_lsq_power_cc3            0.7756 # avg power usage of instruction lsq_cc3
avg_regfile_power_cc3        0.4652 # avg power usage of arch. regfile_cc3
avg_icache_power_cc3         1.3095 # avg power usage of icache_cc3
avg_dcache_power_cc3         3.0037 # avg power usage of dcache_cc3
avg_dcache2_power_cc3        0.7377 # avg power usage of dcache2_cc3
avg_alu_power_cc3            2.5552 # avg power usage of alu_cc3
avg_resultbus_power_cc3       1.7284 # avg power usage of resultbus_cc3
avg_clock_power_cc3          6.9253 # avg power usage of clock_cc3
fetch_stage_power_cc3  12548782.6272 # total power usage of fetch stage_cc3
dispatch_stage_power_cc3 1103209.6019 # total power usage of dispatch stage_cc3
issue_stage_power_cc3  84613287.5203 # total power usage of issue stage_cc3
avg_fetch_power_cc3          1.7899 # average power of fetch unit per cycle_cc3
avg_dispatch_power_cc3       0.1574 # average power of dispatch unit per cycle_cc3
avg_issue_power_cc3         12.0688 # average power of issue unit per cycle_cc3
total_power_cycle_cc3  150079929.4961 # total power per cycle_cc3
avg_total_power_cycle_cc3      21.4066 # average total power per cycle_cc3
avg_total_power_insn_cc3      30.0156 # average total power per insn_cc3
total_rename_access         5000069 # total number accesses of rename unit
total_bpred_access           108695 # total number accesses of bpred unit
total_window_access        25326372 # total number accesses of instruction window
total_lsq_access            3586999 # total number accesses of load/store queue
total_regfile_access        4782612 # total number accesses of arch. regfile
total_icache_access         5000085 # total number accesses of icache
total_dcache_access         3586988 # total number accesses of dcache
total_dcache2_access         652177 # total number accesses of dcache2
total_alu_access            4891361 # total number accesses of alu
total_resultbus_access      7608776 # total number accesses of resultbus
avg_rename_access            0.7132 # avg number accesses of rename unit
avg_bpred_access             0.0155 # avg number accesses of bpred unit
avg_window_access            3.6124 # avg number accesses of instruction window
avg_lsq_access               0.5116 # avg number accesses of lsq
avg_regfile_access           0.6822 # avg number accesses of arch. regfile
avg_icache_access            0.7132 # avg number accesses of icache
avg_dcache_access            0.5116 # avg number accesses of dcache
avg_dcache2_access           0.0930 # avg number accesses of dcache2
avg_alu_access               0.6977 # avg number accesses of alu
avg_resultbus_access         1.0853 # avg number accesses of resultbus
max_rename_access                 4 # max number accesses of rename unit
max_bpred_access                  1 # max number accesses of bpred unit
max_window_access                16 # max number accesses of instruction window
max_lsq_access                    2 # max number accesses of load/store queue
max_regfile_access                4 # max number accesses of arch. regfile
max_icache_access                 4 # max number accesses of icache
max_dcache_access                 4 # max number accesses of dcache
max_dcache2_access                3 # max number accesses of dcache2
max_alu_access                    3 # max number accesses of alu
max_resultbus_access              5 # max number accesses of resultbus
max_cycle_power_cc1         56.3291 # maximum cycle power usage of cc1
max_cycle_power_cc2         47.4982 # maximum cycle power usage of cc2
max_cycle_power_cc3         51.8737 # maximum cycle power usage of cc3


******* SMT STATS *******
THROUGHPUT IPC: 0.7132

